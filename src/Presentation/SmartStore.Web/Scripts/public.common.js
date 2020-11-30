(function ($, window, document, undefined) {

    var viewport = ResponsiveBootstrapToolkit;

    window.displayAjaxLoading = function (display) {
        if ($.throbber === undefined)
            return;

        if (display) {
            $.throbber.show({ speed: 50, white: true });
        }
        else {
            $.throbber.hide();
        }
    };

    window.getPageWidth = function () {
        return parseFloat($("#page").css("width"));
    };

    window.getViewport = function () {
        return viewport;
    };

    window.CookieManager = {
        getCookie: function (cookieName) {
            var name = cookieName + "=";
            var decodedCookie = decodeURIComponent(document.cookie);
            var ca = decodedCookie.split(';');
            for (var i = 0; i < ca.length; i++) {
                var c = ca[i];
                while (c.charAt(0) == ' ') {
                    c = c.substring(1);
                }
                if (c.indexOf(name) == 0) {
                    return c.substring(name.length, c.length);
                }
            }
            return "";
        },
        isAllowed: function (propName) {
            var cookie = this.getCookie("CookieConsent");
            if (cookie !== "") {
                var obj = JSON.parse(cookie);
                return obj[propName];
            }
            return false;
        },
        get allowsAnalytics() {
            return this.isAllowed("AllowAnalytics");
        },
        get allowsThirdParty() {
            return this.isAllowed("AllowThirdParty");
        }
    };

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
            var selector = Modernizr.touchevents
                ? '[data-toggle=tooltip].tooltip-toggle-touch, .tooltip-toggle.tooltip-toggle-touch'
                : '[data-toggle=tooltip], .tooltip-toggle';
            ctx.tooltip({
                selector: selector,
                animation: false,
                trigger: 'hover'
            });
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
        // newsletter subsription
        function (ctx) {
            var newsletterContainer = $(".footer-newsletter");
            if (newsletterContainer.length > 0) {
                var url = newsletterContainer.data("subscription-url");

                newsletterContainer.find('#newsletter-subscribe-button').on("click", function () {
                    var email = $("#newsletter-email").val();
                    var subscribe = 'true';
                    var resultDisplay = $("#newsletter-result-block");
                    var elemGdprConsent = $(".footer-newsletter .gdpr-consent-check");
                    var gdprConsent = elemGdprConsent.length == 0 ? null : elemGdprConsent.is(':checked');

                    if ($('#newsletter-unsubscribe').is(':checked')) {
                        subscribe = 'false';
                    }

                    $.ajax({
                        cache: false,
                        type: "POST",
                        url: url,
                        data: { "subscribe": subscribe, "email": email, "GdprConsent": subscribe == 'true' ? gdprConsent : true },
                        success: function (data) {
                            resultDisplay.html(data.Result);
                            if (data.Success) {
                                $('#newsletter-subscribe-block').hide();
                                resultDisplay.removeClass("alert-danger d-none").addClass("alert-success d-block");
                            }
                            else {
                                if (data.Result != "")
                                    resultDisplay.removeClass("alert-success d-none").addClass("alert-danger d-block").fadeIn("slow").delay(2000).fadeOut("slow");
                            }
                        },
                        error: function (xhr, ajaxOptions, thrownError) {
                            resultDisplay.empty()
                                .text(newsletterContainer.data('subscription-failure'))
                                .removeClass("alert-success d-none")
                                .addClass("alert-danger d-block");
                        }
                    });
                    return false;
                });
            }
        },
        // cookie manager
        function (ctx) {
            ctx.find('.cookie-manager').on("click", function (e) {
                e.preventDefault();

                var dialog = $("#cookie-manager-window");

                if (dialog.length > 0) {
                    // Dialog was already loaded > just open dialog.
                    $('#cookie-manager-window').modal('show');
                }
                else {

                    // Dialog wasn't loaded yet > get view via ajax call.
                    var url = $(this).attr("href");

                    $.ajax({
                        cache: false,
                        type: "POST",
                        url: url,
                        success: function (data) {
                            $("body").append(data);
                        }
                    });
                }
            });
        },
        // slick carousel
        function (ctx) {
            if ($.fn.slick === undefined)
                return;

            ctx.find('.artlist-carousel > .artlist-grid').each(function (i, el) {
                var list = $(this);
                var slidesToShow = list.data("slides-to-show");
                var slidesToScroll = list.data("slides-to-scroll");

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
                    slidesToShow: slidesToShow || 6,
                    slidesToScroll: slidesToScroll || 6,
                    responsive: [
                        {
                            breakpoint: 280,
                            settings: {
                                slidesToShow: Math.min(slidesToShow || 1, 1),
                                slidesToScroll: Math.min(slidesToScroll || 1, 1)
                            }
                        },
                        {
                            breakpoint: 440,
                            settings: {
                                slidesToShow: Math.min(slidesToShow || 2, 2),
                                slidesToScroll: Math.min(slidesToScroll || 2, 2)
                            }
                        },
                        {
                            breakpoint: 640,
                            settings: {
                                slidesToShow: Math.min(slidesToShow || 3, 3),
                                slidesToScroll: Math.min(slidesToScroll || 3, 3)
                            }
                        },
                        {
                            breakpoint: 780,
                            settings: {
                                slidesToShow: Math.min(slidesToShow || 4, 4),
                                slidesToScroll: Math.min(slidesToScroll || 4, 4)
                            }
                        },
                        {
                            breakpoint: 960,
                            settings: {
                                slidesToShow: Math.min(slidesToShow || 5, 5),
                                slidesToScroll: Math.min(slidesToScroll || 5, 5)
                            }
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

    window.applyCommonPlugins = function (/* jQuery */ context) {
        $.each(_commonPluginFactories, function (i, val) {
            val.call(this, $(context));
        });
    };

    // on document ready
    // TODO: reorganize > public.globalinit.js
    $(function () {
        // Init reveal on scroll with AOS library
        if (typeof AOS !== 'undefined' && !$('body').hasClass('no-reveal')) {
            AOS.init({ once: true, duration: 1000 });
        }

        if (SmartStore.parallax !== undefined && !$('body').hasClass('no-parallax')) {
            SmartStore.parallax.init({
                context: document.body,
                selector: '.parallax'
            });
        }

        applyCommonPlugins($("body"));
    });

})(jQuery, this, document);

