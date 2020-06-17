/*
*  Project: Parallax scrolling effect
*  Author: Murat Cakir, SmartStore AG
*  Date: 15.10.2018
*/

; (function ($, window, document, undefined) {

    var initialized = false,
        isTouch = Modernizr.touchevents,
        stages = [],
        //// to adjust speed on smaller devices
        //speedRatios = { xs: 0.5, sm: 0.6, md: 0.7, lg: 0.9, xl: 1 },
        viewport = ResponsiveBootstrapToolkit;

    function update() {
        _.each(stages, function (item, i) {
            if (item.type == 'bg' && isTouch) {
                // Found no proper way to make bg parallax
                // run reliably on touch devices.
                return;
            }            

            var el = $(item.el);
            var winHeight = window.innerHeight;
            var scrollTop = window.pageYOffset;
            var top = el.offset().top;
            var height = el.outerHeight(false);

            // Check if totally above or totally below viewport
            var visible = !(top + height < scrollTop || top > scrollTop + winHeight);

            if (!visible)
                return;

            if (item.filter && !viewport.is(item.filter)) {
                if (item.initialized) {
                    // Restore original styling
                    el.css('background-position', item.originalPosition);
                    el.css('background-attachment', item.originalAttachment);
                    item.initialized = false;
                }

                return;
            }             

            speed = item.speed; // * speedRatios[viewport.current()];

            if (item.type === 'bg') {
                if (!item.initialized) {
                    // for smoother scrolling
                    el.css('background-attachment', 'fixed');
                    item.initialized = true;
                }

                // set bg parallax offset
                var ypos = Math.round((top - scrollTop) * speed) + (item.offset * -1);
                el.css('background-position', 'center ' + ypos + "px");
            }
            else if (item.type === 'content') {
                var bottom = top + height,
                    rate = 100 / (bottom + winHeight - top) * ((scrollTop + winHeight) - top),
                    ytransform = (rate - 50) * (speed * -6) + item.offset;

                el.css(window.Prefixer.css('transform'), 'translate3d(0, ' + ytransform + 'px, 0)');
            }
        });
    }

    var Parallax = SmartStore.parallax = {
        init: function (options /*{ context: Element, selector: string}*/) {
            var opts = options || {};
            var ctx = $(opts.context || document.body);
            var selector = opts.selector || '.parallax';

            stages = _.map(ctx.find(selector).toArray(), function (val, key) {
                var el = $(val);
                return {
                    el: val,
                    type: el.data('parallax-type') || 'bg',
                    speed: toFloat(el.data('parallax-speed'), 0.5),
                    offset: (el.data('parallax-offset') || 0),
                    filter: el.data('parallax-filter'),
                    originalPosition: el.css('background-position'),
                    originalAttachment: el.css('background-attachment'),
                    initialized: false
                };
            });

            if (!initialized) {
                $(window).on('resize scroll', update);
                update();
            }
        }
    };

})(jQuery, window, document);
