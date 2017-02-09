/*
*  Project: Responsive nav/tabs 
*  Author: Murat Cakir, SmartStore AG
*  Date: 16.01.2016
*/

; (function ($, window, document, undefined) {

	var pluginName = 'responsiveNav';
	var viewport = ResponsiveBootstrapToolkit;

	function ResponsiveNav(element, options) {
		var self = this;

		this.element = element;
		var el = this.el = $(element);
		var opts = this.options = $.extend({}, options);

		function isOffCanvas() {
			return el.hasClass('offcanvas');
		}

		function collapseNav() {
			if (el.data('offcanvas')) return;

			// create offcanvas wrapper
			var offcanvas = $('<aside class="offcanvas offcanvas-right offcanvas-overlay offcanvas-fullscreen" data-overlay="true"><div class="offcanvas-content"></div></aside>').appendTo('body');

			// handle .offcanvas-closer click
			offcanvas.on('click', '.offcanvas-closer', function (e) {
				offcanvas.offcanvas('hide');
			});

			// put .tab-content into offcanvas wrapper
			var tabContent = el.find('.tab-content');
			tabContent.appendTo(offcanvas.children().first());
			el.data('tab-content', tabContent).data('offcanvas', offcanvas).addClass('collapsed');

			el.find('.nav .nav-item')
		        .attr('data-toggle', 'offcanvas')
		        .attr('data-placement', 'right')
		        .attr('data-fullscreen', 'true')
		        .attr('data-disablescrolling', 'true')
                .data('target', offcanvas);
		}

		function restoreNav() {
			if (!el.data('offcanvas')) return;

			// move .tab-content back to its origin
			var offcanvas = el.data('offcanvas');
			el.data('tab-content').appendTo(el);
			offcanvas.remove();

			el.removeClass('collapsed').removeData('tab-content').removeData('offcanvas');

			el.find('.nav .nav-item')
		        .removeAttr('data-toggle')
		        .removeAttr('data-placement')
		        .removeAttr('data-fullscreen')
		        .removeAttr('data-disablescrolling')
		        .removeData('target');
		}

		function toggleOffCanvas() {
			var breakpoint = el.data('breakpoint') || '<lg';
			if (viewport.is(breakpoint)) {
				collapseNav();
			}
			else {
				restoreNav();
			}
		}

		this.init = function () {
			EventBroker.subscribe("page.resized", function (msg, viewport) {
				toggleOffCanvas();
			});

			_.delay(toggleOffCanvas, 10);
		};

		this.initialized = false;
		this.init();
		this.initialized = true;
	}

	ResponsiveNav.prototype = {
		// [...]
	}

	// the global, default plugin options
	var defaults = {
		// [...]
	}
	$[pluginName] = { defaults: defaults };


	$.fn[pluginName] = function (options) {
		return this.each(function () {
			if (!$.data(this, pluginName)) {
				options = $.extend({}, $[pluginName].defaults, options);
				$.data(this, pluginName, new ResponsiveNav(this, options));
			}
		});

	}

})(jQuery, window, document);
