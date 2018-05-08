(function ($, window, document, undefined) {

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
        return parseFloat($("#content").css("width"));
    }

    var _commonPluginFactories = [
        // select2
        function (ctx) {
            if (!Modernizr.touchevents) {
                if ($.fn.select2 === undefined || $.fn.selectWrapper === undefined)
                    return;
                ctx.find("select:not(.noskin), input:hidden[data-select]").selectWrapper();
            }
        },
        // tooltips
        function (ctx) {
            if ($.fn.tooltip === undefined)
                return;
            if (!Modernizr.touchevents) {
                ctx.tooltip({ selector: "a[rel=tooltip], .tooltip-toggle" });
            }
        },
        // column equalizer
        function (ctx) {
            if ($.fn.equalizeColumns === undefined)
                return;
<<<<<<< HEAD
            ctx.find(".equalized-column").equalizeColumns({ /*deep: true,*/ responsive: true });
=======
            
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
        // newsletter subsription
        function (ctx) {
            var newsletterContainer = $(".footer-newsletter");
            if (newsletterContainer.length > 0)
            {
                var url = newsletterContainer.data("subscription-url");

                newsletterContainer.find('#newsletter-subscribe-button').on("click", function () {

                    var email = $("#newsletter-email").val();
                    var subscribe = 'true';
                    var resultDisplay = $("#newsletter-result-block");

                    if ($('#newsletter-unsubscribe').is(':checked')) {
                        subscribe = 'false';
                    }
		    
                    $.ajax({
                        cache: false,
                        type: "POST",
                        url: url,
                        data: { "subscribe": subscribe, "email": email },
                        success: function (data) {
                            resultDisplay.html(data.Result);
                            if (data.Success) {
                                $('#newsletter-subscribe-block').hide();
                                resultDisplay.removeClass("alert-danger d-none").addClass("alert-success d-block");
                            }
                            else {
                                resultDisplay.removeClass("alert-success d-none").addClass("alert-danger d-block").fadeIn("slow").delay(2000).fadeOut("slow");
                            }
                        },
                        error:function (xhr, ajaxOptions, thrownError){
                            resultDisplay.empty().text("Failed to subscribe").removeClass("alert-success d-none").addClass("alert-danger d-block");
                        }  
                    });                
                    return false;
                });
            }
        },
        // slick carousel
        function (ctx) {
        	if ($.fn.slick === undefined)
        		return;

        	ctx.find('.artlist-carousel > .artlist-grid').each(function (i, el) {
        		var list = $(this);

        		list.slick({
					infinite: false,
					rtl: $("html").attr("dir") == "rtl",
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
>>>>>>> upstream/3.x
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

        // notify subscribers about page/content width change
        if (window.EventBroker) {
            pageWidth = getPageWidth(); // initial width
            $(window).on("resize", function () {
                // check if page width has changed
                var newWidth = getPageWidth();
                if (newWidth !== pageWidth) {
                    // ...and publish event
                    EventBroker.publish("page.resized", { oldWidth: pageWidth, newWidth: newWidth });
                    pageWidth = newWidth;
                }
            });
        }

        // create navbar
        if ($.fn.navbar)
        {
            $('.navbar ul.nav-smart > li.dropdown').navbar();
        }

        // shrink menu
        if ($.fn.shrinkMenu) {
            $(".shrink-menu").shrinkMenu({ responsive: true });
        }

        
        applyCommonPlugins($("body"));

        //$("select:not(.noskin), input:hidden[data-select]").selectWrapper();

    });

})( jQuery, this, document );

