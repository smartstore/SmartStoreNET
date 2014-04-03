(function ($, window, document, undefined) {

    window.OpenWindow = function(query, w, h, scroll) {
        var l = (screen.width - w) / 2;
        var t = (screen.height - h) / 2;

        winprops = 'resizable=0, height=' + h + ',width=' + w + ',top=' + t + ',left=' + l + 'w';
        if (scroll) winprops += ',scrollbars=1';
        var f = window.open(query, "_blank", winprops);
    }

    window.setLocation = function(url) {
        window.location.href = url;
    }

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

    window.displayNotification = function(message, type, sticky, delay) {
        if (window.EventBroker === undefined || window._ === undefined)
            return;

        var notify = function (msg) {
            EventBroker.publish("message", {
                text: msg,
                type: type,
                delay: delay || 5000,
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

    window.htmlEncode = function(value) {
        return $('<div/>').text(value).html();
    }

    window.htmlDecode = function(value) {
        return $('<div/>').html(value).text();
    }

    window.getPageWidth = function() {
        return parseFloat($("#content").css("width"));
    }

    // codehint: sm-add
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

    // global notification subscriber
    if (window.EventBroker && window._ && $.pnotify) {
    	//var stack_bottomright = { "dir1": "up", "dir2": "left", "firstpos1": 25, "firstpos2": 25 };
    	var stack_topright = { "dir1": "down", "dir2": "left", "firstpos1": 60 };
        EventBroker.subscribe("message", function (message, data) {
            var opts = _.isString(data) ? { text: data } : data;

            opts.stack = stack_topright;
            //opts.addclass = "stack-bottomright";

            $.pnotify(opts);
        });
    }

    // on document ready
    // TODO: reorganize > public.globalinit.js
    $(function () {

        if (!Modernizr.csstransitions) {
            $.fn.transition = $.fn.animate;
        }

        // adjust pnotify global defaults
        if ($.pnotify) {
            $.extend($.pnotify.defaults, {
                history: false,
                animate_speed: "fast"
            });

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

