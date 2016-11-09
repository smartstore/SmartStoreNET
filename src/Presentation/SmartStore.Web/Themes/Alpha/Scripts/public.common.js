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
            if (!Modernizr.touch) {
                if ($.fn.select2 === undefined || $.fn.selectWrapper === undefined)
                    return;
                ctx.find("select:not(.noskin), input:hidden[data-select]").selectWrapper();
            }
        },
        // tooltips
        function (ctx) {
            if ($.fn.tooltip === undefined)
                return;
            if (!Modernizr.touch) {
                ctx.tooltip({ selector: "a[rel=tooltip], .tooltip-toggle" });
            }
        },
        // column equalizer
        function (ctx) {
            if ($.fn.equalizeColumns === undefined)
                return;
            ctx.find(".equalized-column").equalizeColumns({ /*deep: true,*/ responsive: true });
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

