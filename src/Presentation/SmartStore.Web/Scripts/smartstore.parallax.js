/*
*  Project: Parallax scrolling effect
*  Author: Murat Cakir, SmartStore AG
*  Date: 15.10.2018
*/

; (function ($, window, document, undefined) {

    //var noParallax = $('body').hasClass('no-parallax'),
    //    win = $(window),
    //    winHeight = win.height();

    //$('.parallax').each(function (i, el) {

    //});

    var initialized = false,
        stages = [],
        win,
        winHeight,
        scrollTop,
        viewport = ResponsiveBootstrapToolkit;

    function update() {
        _.each(stages, function (val, i) {
            var el = $(val);

            var top = el.offset().top;
            var height = el.outerHeight(false);

            // Check if totally above or totally below viewport
            if (top + height < scrollTop || top > scrollTop + winHeight) {
                return;
            }

            if (el.data('parallax-filter')) {
                if (!viewport.is(el.data('parallax-filter'))) {
                    return;
                }
            }

            // set bg parallax offset
            var offset = (el.data('parallax-offset') || 0) * -1;
            var speedFactor = el.data('parallax-speed') || 0.5;
            var ypos = Math.round((top - scrollTop) * speedFactor * -1) + offset;
            el.css('background-position-y', ypos + "px");
        });
    }

    var Parallax = SmartStore.parallax = {
        init: function (options /*{ context: Element, selector: string}*/) {
            var opts = options || {};
            var ctx = $(opts.context || document.body);
            var selector = opts.selector || '.parallax';

            stages = ctx.find(selector).toArray();

            if (!initialized) {
                win = $(window);
                winHeight = win.height();
                scrollTop = win.scrollTop();

                win.on('resize', function () {
                    winHeight = win.height();
                    update();
                });

                win.on('scroll', function () {
                    scrollTop = win.scrollTop();
                    update();
                });

                win.on('scroll', update);
                update();
                initialized = true;
            }
        }
    };

    //SmartStore.parallax = {
    //    init: function (ctx, selector) {
    //        if (noParallax)
    //            return;

    //        selector = selector || '.parallax';
    //    }
    //};

    // PARALLAX DATA API
    // ======================================================

    $(function () {
        SmartStore.parallax.init();
    });

})(jQuery, window, document);
