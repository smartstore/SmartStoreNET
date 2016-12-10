(function ($, window, document, undefined) {

	var viewport = ResponsiveBootstrapToolkit;

    window.displayAjaxLoading = function(display) {
        if ($.throbber === undefined)
            return;

        if (display) {
            $.throbber.show({ speed: 50, white: true });
        }
        else {
            $.throbber.hide();
        }
    }

    window.getPageWidth = function() {
        return parseFloat($("#page").css("width"));
    }

    window.getViewport = function () {
    	return viewport;
    }

    var _commonPluginFactories = [
        // select2
        function (ctx) {
            if (!Modernizr.touch) {
                if ($.fn.select2 === undefined || $.fn.selectWrapper === undefined)
                    return;
                //ctx.find("select:not(.noskin), input:hidden[data-select]").selectWrapper();
            }
        },
        // tooltips
        function (ctx) {
            if ($.fn.tooltip === undefined)
                return;
            if (!Modernizr.touch) {
                ctx.tooltip({ selector: '[data-toggle="tooltip"], .tooltip-toggle', container: 'body' });
            }
        }
    ];

    /* 
        Helpful in AJAX scenarios, where jQuery plugins has to be applied 
        to newly created html.
    */
    
    window.applyCommonPlugins = function(/* jQuery */ context) {
        $.each(_commonPluginFactories, function (i, val) {
            val.call(this, $(context));
        });
    }

    // on document ready
    // TODO: reorganize > public.globalinit.js
    $(function () {

        // adjust pnotify global defaults
        if ($.pnotify) {

            // intercept window.alert with pnotify
            window.alert = function (message) {
                if (message == null || message.length <= 0)
                    return;

                $.pnotify({
                    title: window.Res["Common.Notification"],
                    text: message,
                    type: "info",
                    animate_speed: 'fast',
                    closer_hover: false,
                    stack: false,
                    before_open: function (pnotify) {
                        // Position this notice in the center of the screen.
                        pnotify.css({
                            "top": ($(window).height() / 2) - (pnotify.height() / 2),
                            "left": ($(window).width() / 2) - (pnotify.width() / 2)
                        });
                    }
                });
            }
        }

        // Notify subscribers about page/content width change
        if (window.EventBroker) {
        	$(window).resize(
				viewport.changed(function () {
					var tier = viewport.current();
					console.debug("Grid tier changed: " + tier);
					EventBroker.publish("page.resized", viewport);
				}, 100)
			);
        }

    	// create navbar
    	// TODO: (mc) away with it (?)
        if ($.fn.navbar)
        {
            $('.navbar ul.nav-smart > li.dropdown').navbar();
        }

    	// shrink menu 
		// TODO: (mc) away with it! 
        if ($.fn.shrinkMenu) {
            $(".shrink-menu").shrinkMenu({ responsive: true });
        }
        
        applyCommonPlugins($("body"));

        //$("select:not(.noskin), input:hidden[data-select]").selectWrapper();

    });

})( jQuery, this, document );

