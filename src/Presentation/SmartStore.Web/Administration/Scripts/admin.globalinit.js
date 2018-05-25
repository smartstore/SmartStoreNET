/// <reference path="admin.common.js" />

(function ($, window, document, undefined) {

	var _commonPluginFactories = [
		// panel toggling
		function (ctx) {
			ctx.find('input[type=checkbox][data-toggler-for]').each(function (i, el) {
				SmartStore.Admin.togglePanel(el, false);
			});
		},
		// select2
		function (ctx) {
			ctx.find("select:not(.noskin)").selectWrapper();
		},
		// tooltips
		function (ctx) {
			ctx.find(".cph").tooltip({
				selector: "a.hint",
				placement: SmartStore.globalization.culture.isRTL ? "right" : "left",
				trigger: 'hover',
				delay: { show: 400, hide: 0 }
			});
		},
		// switch
		function (ctx) {
			ctx.find(".adminData > input[type=checkbox], .multi-store-setting-control > input[type=checkbox]").each(function (i, el) {
				var wrap = $(el)
					.wrap('<label class="switch"></label>')
					.after('<span class="switch-toggle" data-on="' + window.Res['Common.On'] + '" data-off="' + window.Res['Common.Off'] + '"></span>');
			});
		},
		// Telerik
		function (ctx) {
			Hacks.Telerik.handleButton(ctx.find(".t-button").filter(function (index) {
				// reject .t-button that has a .t-group-indicator as parent
				return !$(this).parent().hasClass("t-group-indicator");
			}));

			//// skin telerik grids with bootstrap table (obsolete: styled per Sass @extend now)
			//ctx.find(".t-grid > table").addClass("table");
		},
		// btn-trigger
		function (ctx) {
			// Temp only: delegates anchor clicks to corresponding form-button.
			ctx.find("a[rel='btn-trigger']").click(function () {
				var el = $(this);
				var target = el.data("target");
				var action = el.data("action");
				var button = el.closest("form").find("button[type=submit][name=" + target + "][value=" + action + "]");
				button.click();
				return false;
			});
		},
		// ColorPicker
		function (ctx) {
			ctx.find(".sm-colorbox").colorpicker({ fallbackColor: false, color: false, align: SmartStore.globalization.culture.isRTL ? 'left' : 'right' });
		},
	];


	/* 
	Helpful in AJAX scenarios, where jQuery plugins has to be applied 
	to newly created html.
	*/
	window.applyCommonPlugins = function (/* jQuery */ context) {
		$.each(_commonPluginFactories, function (i, val) {
			val.call(this, $(context));
		});
	};

    $(document).ready(function () {
        var html = $("html");

        html.removeClass("not-ready").addClass("ready");

        applyCommonPlugins($("body"));

    	// Handle panel toggling
        $(document).on('change', 'input[type=checkbox][data-toggler-for]', function (e) {
			SmartStore.Admin.togglePanel(e.target, true);
        });

        $("#page").tooltip({
            selector: "a[rel=tooltip], .tooltip-toggle"
        });

        // Temp only
        $(".options button[value=save-continue]").on('click', function () {
            var btn = $(this);
            btn.closest("form").append('<input type="hidden" name="save-continue" value="true" />');
        });

        // Ajax activity indicator bound to ajax start/stop document events
        $(document).ajaxStart(function () {
            $('#ajax-busy').addClass("busy");
        }).ajaxStop(function () {
            window.setTimeout(function () {
                $('#ajax-busy').removeClass("busy");
            }, 300);
        });

		// check overridden store settings
        $('.multi-store-override-option').each(function (i, el) {
			SmartStore.Admin.checkOverriddenStoreValue(el);
		});

        // publish entity commit messages
        $('.entity-commit-trigger').on('click', function (e) {
            var el = $(this);
            if (el.data('commit-type')) {
                EventBroker.publish("entity-commit", {
                    type: el.data('commit-type'),
                    action: el.data('commit-action'),
                    id: el.data('commit-id')
                });
            }
		});

		// Because we restyled the grid, the filter dropdown does not position
		// correctly anymore. We have to reposition it.
		Hacks.Telerik.handleGridFilter();

        // sticky section-header
        var navbar = $("#navbar");
        var navbarHeight = navbar.height() || 1;
        var sectionHeader = $('.section-header');
        var sectionHeaderHasButtons = undefined;

        $(window).on("scroll resize", function (e) {
            if (sectionHeaderHasButtons === undefined) {
                sectionHeaderHasButtons = sectionHeader.find(".options").children().length > 0;
            }
            if (sectionHeaderHasButtons === true) {
            	var y = $(this).scrollTop();
                sectionHeader.toggleClass("sticky", y >= navbarHeight);
            }
        }).trigger('resize');

        $(window).on('load', function () {

        	// swap classes onload and domready
        	html.removeClass("loading").addClass("loaded");

        	// make #content fit into viewspace
        	var fitContentToWindow = function (initial) {
        		var content = $('#content');

        		if (!content.length)
        			return;

        		var height = initialHeight = content.height(),
                             outerHeight,
                             winHeight = $(document).height(),
                             top,
                             offset;

        		if (initial === true) {
        			top = content.offset().top;
        			offset = content.outerHeight(false) - content.height();
        			if ($('html').hasClass('wkit')) offset += 2; // dont know why!
        			content.data("initial-height", initialHeight)
                                       .data("initial-top", top)
                                       .data("initial-offset", offset);
        		}
        		else {
        			top = content.data("initial-top");
        			offset = content.data("initial-offset");
        			initialHeight = content.data("initial-height");
        		}

        		content.css("min-height", Math.max(initialHeight, winHeight - offset - top) + "px");

			};

			if (!$('body').is('.popup.bare')) {
				fitContentToWindow(true);
				$(window).on("resize", fitContentToWindow);
			}

        });

    });


})( jQuery, this, document );