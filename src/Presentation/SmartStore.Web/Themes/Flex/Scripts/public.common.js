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
            if ($.fn.select2 === undefined || $.fn.selectWrapper === undefined)
            	return;
            ctx.find("select:not(.noskin), input:hidden[data-select]").selectWrapper();
        },
        // tooltips
        function (ctx) {
            if ($.fn.tooltip === undefined)
                return;
            if (!Modernizr.touchevents) {
                ctx.tooltip({ selector: '[data-toggle="tooltip"], .tooltip-toggle', container: 'body' });
            }
        },
        // touch spin
        function (ctx) {
            if ($.fn.TouchSpin === undefined)
                return;
            
            ctx.find('.qty-input > .form-control').each(function (i, el) {
                var ctl = $(this);
                
                ctl.TouchSpin({
                    buttondown_class: 'btn btn-secondary',
                    buttonup_class: 'btn btn-secondary',
                    buttondown_txt: '<i class="fa fa-minus"></i>',
                    buttonup_txt: '<i class="fa fa-plus"></i>',
                });
            });
        },
        // slick carousel
        function (ctx) {
        	if ($.fn.slick === undefined)
        		return;

        	ctx.find('.artlist-carousel > .artlist-grid').each(function (i, el) {
        		var list = $(this);

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
        // Notify subscribers about page/content width change
        if (window.EventBroker) {
        	var currentContentWidth = $('#content').width();
        	$(window).on('resize', function () {
        		var contentWidth = $('#content').width();
        		if (contentWidth !== currentContentWidth) {
        			currentContentWidth = contentWidth;
        			console.debug("Grid tier changed: " + viewport.current());
        			EventBroker.publish("page.resized", viewport);
        		}
        	});
        }
        
        applyCommonPlugins($("body"));

        //$("select:not(.noskin), input:hidden[data-select]").selectWrapper();

    });

})( jQuery, this, document );

