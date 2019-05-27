(function ($, window, document, undefined) {
	var viewport = ResponsiveBootstrapToolkit;

	// TODO: (mc) ABS4 > delete viewport specific stuff from ~/Scripts/public.common.js, it's shared now.'
	window.getPageWidth = function () {
		return parseFloat($("#page").css("width"));
	}

	window.getViewport = function () {
		return viewport;
	}

	window.setLocation = function (url) {
		window.location.href = url;
	}

	window.openPopup = function (url, large, flex) {
		var opts = $.isPlainObject(url) ? url : {
			/* id, backdrop */
			url: url,
			large: large,
			flex: flex
		};

		var id = (opts.id || "modal-popup-shared");
		var modal = $('#' + id);
		var sizeClass = "";

		if (opts.flex === undefined) opts.flex = true;
		if (opts.flex) sizeClass = "modal-flex";
		if (opts.backdrop === undefined) opts.backdrop = true;

		if (opts.large && !opts.flex)
			sizeClass = "modal-lg";
		else if (!opts.large && opts.flex)
			sizeClass += " modal-flex-sm";

		if (modal.length === 0) {
			var html = [
				'<div id="' + id + '" class="modal fade" data-backdrop="' + opts.backdrop + '" role="dialog" aria-hidden="true" tabindex="-1" style="border-radius: 0">',
					'<a href="javascript:void(0)" class="modal-closer d-none d-md-block" data-dismiss="modal" title="' + window.Res['Common.Close'] + '">&times;</a>',
					'<div class="modal-dialog{0} modal-dialog-app" role="document">'.format(!!(sizeClass) ? " " + sizeClass : ""),
						'<div class="modal-content">',
							'<div class="modal-body">',
								'<iframe class="modal-flex-fill-area" frameborder="0" src="' + opts.url + '" />',
							'</div>',
							'<div class="modal-footer d-md-none">',
								'<button type="button" class="btn btn-secondary btn-sm btn-default" data-dismiss="modal">' + window.Res['Common.Close'] + '</button>',
							'</div>',
						'</div>',
					'</div>',
				'</div>'
			].join("");

            modal = $(html).appendTo('body').on('hidden.bs.modal', function (e) {
                // Cleanup
                $(modal.find('iframe').attr('src', 'about:blank')).remove();
				modal.remove();
			});

			// Create spinner
			var spinner = $('<div class="spinner-container w-100 h-100 active" style="position:absolute; top:0; background:#fff; border-radius:4px"></div>').append(createCircularSpinner(64, true, 2));
			modal.find('.modal-body').append(spinner);

			modal.find('.modal-body > iframe').on('load', function (e) {
				modal.find('.modal-body > .spinner-container').removeClass('active');
			});
		}
		else {
			var iframe = modal.find('.modal-body > iframe');
			modal.find('.modal-body > .spinner-container').addClass('active');
			iframe.attr('src', opts.url);
		}

		modal.modal('show');
	}

	window.closePopup = function (id) {
		var modal = $('#' + (id || "modal-popup-shared"));
		if (modal.length > 0) {
			modal.modal('hide');
		}
	}

	window.openWindow = function (url, w, h, scroll) {
		w = w || (screen.availWidth - (screen.availWidth * 0.25));
		h = h || (screen.availHeight - (screen.availHeight * 0.25));

		var l = (screen.availLeft + (screen.availWidth / 2)) - (w / 2);
		var t = (screen.availTop + (screen.availHeight / 2)) - (h / 2);

		winprops = 'dependent=1,resizable=0,height=' + h + ',width=' + w + ',top=' + t + ',left=' + l;
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
	        strokeWidth = 4;
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
				elFocus = document.activeElement,
				elContext = elFocus || document.body;

			textarea.textContent = text;
			textarea.style.position = 'fixed';
			textarea.style.width = '10px';
			textarea.style.height = '10px';

			elContext.appendChild(textarea);

			textarea.focus();
			textarea.setSelectionRange(0, textarea.value.length);

			try {
				result = document.execCommand('copy');
			}
			catch (e) {
			}
			finally {
				elContext.removeChild(textarea);
				if (elFocus) {
					elFocus.focus();
				}
			}
		}
		return result;
	}

	window.getImageSize = function (url, callback) {
		var img = new Image();
		img.src = url;
		img.onload = function () {
			callback.apply(this, [img.naturalWidth, img.naturalHeight]);
		};
	}

    window.renderGoogleRecaptcha = function (containerId, sitekey, invisible) {
        var frm = $('#' + containerId).closest('form');

        if (frm.length === 0)
            return;

        var holderId = grecaptcha.render(containerId, {
            sitekey: sitekey,
            size: invisible ? 'invisible' : undefined,
            badge: 'bottomleft',
            callback: function (token) {
                if (invisible && frm) {
                    frm[0].submit();
                }
            }
        });

        if (invisible) {
            frm.on('submit', function (e) {
                if ($.validator === undefined || frm.valid() == true) {
                    e.preventDefault();
                    grecaptcha.execute(holderId);
                }
            });
        }
    }

    // on document ready
	$(function () {
        var rtl = SmartStore.globalization != undefined ? SmartStore.globalization.culture.isRTL : false,
            win = $(window),
            body = $(document.body);

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
			var stack = {
				"dir1": "down",
				"dir2": rtl ? "right" : "left",
				"push": "bottom",
				"firstpos1": $('html').data('pnotify-firstpos1') || 80,
				"spacing1": 0, "spacing2": 25, "context": $("body")
			};
			PNotify.prototype.options = $.extend(PNotify.prototype.options, {
				styling: "fontawesome",
				stack: stack,
				addclass: 'stack-top' + (rtl ? 'left' : 'right'),
				width: "450px",
				mobile: { swipe_dismiss: true, styling: true },
				animate: {
					animate: true,
					in_class: "fadeInDown",
					out_class: "fadeOut" + (rtl ? 'Left' : 'Right')
				}
			});
		}

		// Adjust datetimepicker global defaults
		var dtp = $.fn.datetimepicker;
		if (typeof dtp !== 'undefined' && dtp.Constructor && dtp.Constructor.Default) {
			dtp.Constructor.Default = $.extend({}, dtp.Constructor.Default, {
				locale: 'glob',
				keepOpen: false,
				collapse: true,
				widgetPositioning: {
					horizontal: 'right',
					vertical: 'auto'
				},
				icons: {
					time: 'far fa-clock',
					date: 'fa fa-calendar',
					up: 'fa fa-angle-up',
					down: 'fa fa-angle-down',
					previous: 'fa fa-angle-left',
					next: 'fa fa-angle-right',
					today: 'far fa-calendar-check',
					clear: 'fa fa-delete',
					close: 'fa fa-times'
				}
			});
		}

		// Global notification subscriber
		if (window.EventBroker && window._ && typeof PNotify !== 'undefined') {
			var stack_bottomcenter = { "dir1": "up", "dir2": "right", "firstpos1": 100, "firstpos2": 10 };
			EventBroker.subscribe("message", function (message, data) {
				var opts = _.isString(data) ? { text: data } : data;
				new PNotify(opts);
			});
		}

		// tab strip smart auto selection
		$('.tabs-autoselect ul.nav a[data-toggle=tab]').on('shown.bs.tab', function (e) {
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
		$('.nav a[data-ajax-url]').on('show.bs.tab', function (e) {
			var newTab = $(e.target),
				tabbable = newTab.closest('.tabbable'),
				pane = tabbable.find(newTab.attr("href")),
				url = newTab.data('ajax-url');

			if (newTab.data("loaded") || !url)
				return;

			$.ajax({
				cache: false,
				type: "GET",
				async: true,
				global: false,
				url: url,
				beforeSend: function (xhr) {
					pane.html($("<div class='text-center mt-6'></div>").append(createCircularSpinner(48, true, 2)));
					getFunction(tabbable.data("ajax-onbegin"), ["tab", "pane", "xhr"]).apply(this, [newTab, pane, xhr]);
				},
				success: function (data, status, xhr) {
					pane.html(data);
					getFunction(tabbable.data("ajax-onsuccess"), ["tab", "pane", "data", "status", "xhr"]).apply(this, [newTab, pane, data, status, xhr]);
				},
				error: function (xhr, ajaxOptions, thrownError) {
					pane.html('<div class="text-danger">Error while loading resource: ' + thrownError + '</div>');
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
		(function () {
			$('.mf-dropdown').each(function (i, el) {
				var elLabel = $('> .btn [data-bind]', el);
				if (elLabel.length == 0 || elLabel.text().length > 0)
					return;

				var sel = $('select > option:selected', el).text() || $('select > option', el).first().text();
				elLabel.text(sel);
			});

			body.on('mouseenter mouseleave mousedown change', '.mf-dropdown > select', function (e) {
				var btn = $(this).parent().find('> .btn');
				if (e.type == "mouseenter") {
					btn.addClass('hover');
				}
				else if (e.type == "mousedown") {
					btn.addClass('active focus').removeClass('hover');
					_.delay(function () {
                        body.one('mousedown touch', function (e) { btn.removeClass('active focus'); });
					}, 50);
				}
				else if (e.type == "mouseleave") {
					btn.removeClass('hover');
				}
				else if (e.type == "change") {
					btn.removeClass('hover active focus');
					var elLabel = btn.find('[data-bind]');
					elLabel.text(elLabel.data('bind') == 'value' ? $(this).val() : $('option:selected', this).text());
				}
			});
		})();
		

		(function () {
			var currentDrop,
				currentSubDrop,
				closeTimeout,
				closeTimeoutSub;

			function closeDrop(drop, fn) {
				drop.removeClass('show').find('> .dropdown-menu').removeClass('show');
				if (_.isFunction(fn)) fn();
			}

			// drop dropdown menus on hover
			$(document).on('mouseenter mouseleave', '.dropdown-hoverdrop', function (e) {
				var li = $(this),
					a = $('> .dropdown-toggle', this);

				if (a.data("toggle") === 'dropdown')
					return;

				var afterClose = function () { currentDrop = null; };

				if (e.type == 'mouseenter') {
					if (currentDrop) {
						clearTimeout(closeTimeout);
						closeDrop(currentDrop, afterClose);
					}
					li.addClass('show').find('> .dropdown-menu').addClass('show');
					currentDrop = li;
				}
				else {
					li.removeClass('show');
					closeTimeout = window.setTimeout(function () { closeDrop(li, afterClose); }, 250);
				}
			});

			// handle nested dropdown menus
			$(document).on('mouseenter mouseleave', '.dropdown-group', function (e) {
				var li = $(this);

				if (e.type == 'mouseenter') {
					if (currentSubDrop) {
						clearTimeout(closeTimeoutSub);
						closeDrop(currentSubDrop);
					}
					li.addClass('show').find('> .dropdown-menu').addClass('show');
					currentSubDrop = li;
				}
				else {
					li.removeClass('show');
					closeTimeoutSub = window.setTimeout(function () { closeDrop(li); }, 250);
				}
			});
		})();


		// html text collapser
		if ($.fn.moreLess) {
			$('.more-less').moreLess();
        }

        // Unselectable radio button groups
        $(document).on('click', '.btn-group-toggle.unselectable > .btn', function (e) {
            var btn = $(this);
            var radio = btn.find('input:radio');

            if (radio.length && radio.prop('checked')) {
                _.delay(function () {
                    radio.prop('checked', false);
                    btn.removeClass('active focus');

                    e.preventDefault();
                    e.stopPropagation();
                }, 50);
            }
        });

		// state region dropdown
		$(document).on('change', '.country-selector', function () {
			var el = $(this);
			var selectedItem = el.val();
			var ddlStates = $(el.data("region-control-selector"));
			var ajaxUrl = el.data("states-ajax-url");
			var addEmptyStateIfRequired = el.data("addemptystateifrequired");
			var addAsterisk = el.data("addasterisk");
				
			$.ajax({
				cache: false,
				type: "GET",
				url: ajaxUrl,
				data: { "countryId": selectedItem, "addEmptyStateIfRequired": addEmptyStateIfRequired, "addAsterisk": addAsterisk },
				success: function (data) {
					if (data.error)
						return;

					ddlStates.html('');
					$.each(data, function (id, option) {
						ddlStates.append($('<option></option>').val(option.id).html(option.name));
					});
					ddlStates.trigger("change");
				},
				error: function (xhr, ajaxOptions, thrownError) {
					alert('Failed to retrieve states.');
				}
			});
		});
		
		// scroll top
		(function () {
			$('#scroll-top').on('click', function (e) {
				e.preventDefault();
				win.scrollTo(0, 600);
				return false;
			});

			var prevY;

			var throttledScroll = _.throttle(function (e) {
                var y = win.scrollTop();
				if (_.isNumber(prevY)) {
					// Show scroll button only when scrolled up
					if (y < prevY && y > 500) {
						$('#scroll-top').addClass("in");
					}
					else {
						$('#scroll-top').removeClass("in");
					}
				}

				prevY = y;
			}, 100);

            win.on("scroll", throttledScroll);
        })();
        
        // Modal stuff
        $(document).on('hide.bs.modal', '.modal', function (e) { body.addClass('modal-hiding'); })
        $(document).on('hidden.bs.modal', '.modal', function (e) { body.removeClass('modal-hiding'); })
    });

})( jQuery, this, document );

