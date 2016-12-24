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
        },
        // slick carousel
        function (ctx) {
        	if ($.fn.slick === undefined)
        		return;

        	ctx.find('.artlist-carousel > .artlist-grid').each(function (i, el) {
        		var list = $(this);

        		var slickData = list.parent().data('slick');
        		
        		if (slickData && list.data('slick') == undefined) {
        			list.data('slick', slickData);
        			console.log(list.data('slick'));
        		}

        		list.slick({
        			infinite: false,
        			dots: true,
        			cssEase: 'ease-in-out',
        			speed: 300,
        			useCSS: true,
        			useTransform: true,
        			waitForAnimate: true,
        			prevArrow: '<button type="button" class="btn btn-secondary slick-prev"><i class="fa fa-chevron-left"></i></button>',
        			nextArrow: '<button type="button" class="btn btn-secondary slick-next"><i class="fa fa-chevron-right"></i></button>',
        			respondTo: 'slider',
        			slidesToShow: 6,
        			slidesToScroll: 6,
        			responsive: [
						{
							breakpoint: 280,
							settings: { slidesToShow: 1, slidesToScroll: 1 }
						},
						{
							breakpoint: 440,
							settings: { slidesToShow: 2, slidesToScroll: 2 }
						},
						{
							breakpoint: 640,
							settings: { slidesToShow: 3, slidesToScroll: 3 }
						},
						{
							breakpoint: 780,
							settings: { slidesToShow: 4, slidesToScroll: 4 }
						},
						{
							breakpoint: 960,
							settings: { slidesToShow: 5, slidesToScroll: 5 }
						},
        			]
        		});
        	});
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
        
        applyCommonPlugins($("body"));

        //$("select:not(.noskin), input:hidden[data-select]").selectWrapper();

    });

})( jQuery, this, document );

