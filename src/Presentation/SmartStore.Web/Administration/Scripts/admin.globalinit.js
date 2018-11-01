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
        // Range slider
        function (ctx) {
            return;
            ctx.find("input[type=range]:not(.noskin)").rangeslider({
                polyfill: false,
                onInit: function () {
                    $rangeEl = this.$range;
                    // add value label to handle
                    var $handle = $rangeEl.find('.rangeslider__handle');
                    var handleValue = '<div class="rangeslider__handle__value">' + this.value + '</div>';
                    $handle.append(handleValue);

                    // get range index labels 
                    var markers = this.$element.data('markers');
                    if (markers) {
                        markers = markers.split(',');

                        // add labels
                        $rangeEl.append('<div class="rangeslider__labels"></div>');
                        $(markers).each(function (index, value) {
                            $rangeEl.find('.rangeslider__labels').append('<span class="rangeslider__labels__label">' + value.trim() + '</span>');
                        })
                    }
                },
                onSlide: function (position, value) {
                    var $handle = this.$range.find('.rangeslider__handle__value');
                    $handle.text(this.value);
                },
            });
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
            ctx.find(".adminData > input[type=checkbox], .multi-store-setting-control > input[type=checkbox], .switcher > input[type=checkbox]").each(function (i, el) {
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
        }
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

        if (!sectionHeader.hasClass('nofix')) {
            $(window).on("scroll resize", function (e) {
                if (sectionHeaderHasButtons === undefined) {
                    sectionHeaderHasButtons = sectionHeader.find(".options").children().length > 0;
                }
                if (sectionHeaderHasButtons === true) {
                    var y = $(this).scrollTop();
                    sectionHeader.toggleClass("sticky", y >= navbarHeight);
                }
            }).trigger('resize');
        }

        $(window).on('load', function () {
        	// swap classes onload and domready
        	html.removeClass("loading").addClass("loaded");
        });

    });


})( jQuery, this, document );