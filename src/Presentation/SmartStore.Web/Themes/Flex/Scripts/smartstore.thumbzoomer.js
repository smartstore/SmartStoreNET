/*
*  Project: SmartStore menu 
*  Author: Murat Cakir, SmartStore AG
*/

; (function ($, window, document, undefined) {

    var pluginName = 'thumbZoomer';

    function ThumbZoomer(element, options) {
        var self = this;

        // to access the DOM elem from outside of this constructor
        this.element = element;
        var el = this.el = $(element);

        // support metadata plugin
        var meta = $.metadata ? $.metadata.get(element) : {};
        // 'this.options' ensures we can reference the merged instance options from outside
        var opts = this.options = $.extend({}, options, meta || {});

        this.init = function () {

            if (!el.is('body')) {
                if (/static/.test(el.css("position"))) {
                    el.css("position", "relative");
                }
            }

            // Handle grid thumbs scaling on hover
            el.on("mouseenter", "img.zoomable-thumb", function (e) {
                var img = $(this).css("position", "relative"),
                    offset = img.position(),
                    left = offset.left,
                    top = offset.top,
                    width = img.width(),
                    height = img.height(),
                    realWidth = img[0].naturalWidth,
                    realHeight = img[0].naturalHeight;

                if (realWidth <= width - 5 && realHeight <= height - 5) {
                    // don't scale if thumb real size is insignificantly greater
                    return;
                }

                var clone =
                    img.clone(false)
                       .removeClass("zoomable-thumb")
                       .addClass("zoomable-thumb-clone")
                       .data("original", img[0])
                       .css({
                           position: "absolute",
                           opacity: 0,
                           left: left + "px",
                           top: top + "px",
                           width: width + "px",
                           height: height + "px"
                       })
                       .appendTo(el /*$("body")*/);

                if (opts.setZIndex) {
                    //$.topZIndex(clone);
                    clone.topZIndex();
                }

                function hideClone(el) {
                    el.stop(true, true).transition({
                        width: width,
                        height: height,
                        top: top,
                        left: left
                    }, 100, "ease-in-out", function () { el.remove(); });
                };

                // At first close all clones but the current
                hideClone($(".zoomable-thumb-clone").filter(function (index) {
                    return $(this).data("original") != img[0];
                }));

                clone.on("mouseleave", function () { hideClone(clone); })
                     .stop(true, true)
                     .animate({
                         delay: 200,
                         opacity: 1,
                         width: realWidth,
                         height: realHeight,
                         top: top - ((realHeight - height) / 2),
                         left: left - ((realWidth - width) / 2)
                     }, 250, "ease-in-out");

            });

        };

        this.initialized = false;
        this.init();
        this.initialized = true;
    }

    ThumbZoomer.prototype = {
        // [...]
    }

    // the global, default plugin options
    var defaults = {
        setZIndex: false
    }

    $[pluginName] = { defaults: defaults };


    $.fn[pluginName] = function (options) {

        return this.each(function () {
            if (!$.data(this, pluginName)) {
                options = $.extend({}, $[pluginName].defaults, options);
                $.data(this, pluginName, new ThumbZoomer(this, options));
            }
        });

    }

    /* APPLY TO STANDARD MENU ELEMENTS
	* =================================== */

    $(function () {
        //$('.navbar ul.nav-smart > li.dropdown').menu();
    })

})(jQuery, window, document);
