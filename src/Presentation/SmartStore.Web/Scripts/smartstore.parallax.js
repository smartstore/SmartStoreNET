/*
*  Project: Parallax scrolling effect
*  Author: Murat Cakir, SmartStore AG
*  Date: 15.10.2018
*/

; (function ($, window, document, undefined) {

    var initialized = false,
        isTouch = Modernizr.touchevents,
        elems = [],
        blocks = [],
        scrollTop = 0,
        winHeight = 0,
        pause = true,
        // to adjust speed on smaller devices
        speedRatios = { xs: 0.3, sm: 0.5, md: 0.65, lg: 0.9, xl: 1 },
        viewport = ResponsiveBootstrapToolkit;

    // Check what requestAnimationFrame to use, and if
    // it's not supported, use the onscroll event
    var loop = window.requestAnimationFrame ||
        window.webkitRequestAnimationFrame ||
        window.mozRequestAnimationFrame ||
        window.msRequestAnimationFrame ||
        window.oRequestAnimationFrame ||
        function (callback) { return setTimeout(callback, 1000 / 60); };

    // store the id for later use
    var loopId = null;

    // Test via a getter in the options object to see if the passive property is accessed
    var supportsPassive = false;
    try {
        var opts = Object.defineProperty({}, 'passive', {
            get: function () {
                supportsPassive = true;
            }
        });
        window.addEventListener("testPassive", null, opts);
        window.removeEventListener("testPassive", null, opts);
    } catch (e) {
        //
    }

    // check what cancelAnimation method to use
    var clearLoop = window.cancelAnimationFrame || window.mozCancelAnimationFrame || clearTimeout;

    // check which transform property to use
    var transformProp = window.Prefixer.css('transform');

    function initialize() {
        // Suppressed smooth-parallax for edge per CSS. Observe and leave this commented for now.
        //if ($('html').hasClass('edge'))
        //    return;

        // Reset everything
        for (var i = 0; i < blocks.length; i++) {
            elems[i].style.cssText = blocks[i].style;
        }

        blocks = [];
        winHeight = window.innerHeight;

        setPosition();
        computeBlocks();
        animate();

        // If paused, unpause and set listener for window resizing events
        if (pause) {
            window.addEventListener('resize', initialize);
            pause = false;
            // Start the loop
            update();
        }
    }

    function computeBlocks() {
        for (var i = 0; i < elems.length; i++) {
            var block = computeBlock(elems[i]);
            blocks.push(block);
        }
    }

    // We are going to cache the parallax elements'
    // computed values for performance reasons.
    function computeBlock(el) {
        var $el = $(el);
        var type = $el.data('parallax-type') || 'bg';
        var filter = $el.data('parallax-filter');

        var originalStyle = $(el).data('original-style');
        var style = originalStyle || el.style.cssText;
        if (!originalStyle) {
            $(el).data('original-style', style);
        }

        var originalTransform = $(el).data('original-transform');
        var transform = originalTransform || $el.css('transform') || '';
        if (!originalTransform) {
            $(el).data('original-transform', transform);
        }

        if (transform === 'none') { transform = ''; }

        // Found no proper way to make bg parallax // run reliably on touch devices.
        var paused = (type === 'bg' && isTouch) || (filter && !viewport.is(filter));

        if (paused) {
            return {
                el: el,
                type: type,
                filter: filter,
                style: style,
                transform: transform,
                paused: paused
            };
        }

        var speed = toFloat($el.data('parallax-speed'), 0.5);
        var offset = toFloat($el.data('parallax-offset'), 0);

        // If the element has the percentage attribute, the posY and posX needs to be
        // the current scroll position's value, so that the elements are still positioned based on HTML layout
        var wrapperPosY = window.pageYOffset || document.documentElement.scrollTop || document.body.scrollTop;

        var posY = wrapperPosY;
        var rect = el.getBoundingClientRect();
        var top = posY + rect.top;
        var height = el.clientHeight || el.offsetHeight || el.scrollHeight;
        var bottom = top + height;

        if (type === 'bg') {
            // for smoother scrolling
            $el.css('background-attachment', 'fixed');
        }

        var currentViewport = viewport.current();

        return {
            el: el,
            type: type,
            filter: filter,
            style: style,
            transform: transform,
            paused: paused,
            speed: speed,
            offset: offset,
            top: top,
            height: height,
            bottom: bottom,
            speedRatio: speedRatios[currentViewport]
        };
    }

    // Set scroll position (scrollTop)
    // Returns true if the scroll changed, false if nothing happened
    function setPosition() {
        var oldScrollTop = scrollTop;
        //var oldX = posX;
        var wrapper = document.documentElement || document.body.parentNode || document.body;

        scrollTop = window.pageYOffset || wrapper.scrollTop; // wrapper.scrollTop || window.pageYOffset;

        if (oldScrollTop !== scrollTop) {
            // scroll changed, return true
            return true;
        }

        // scroll did not change
        return false;
    };

    // Remove event listeners and loop again
    function deferredUpdate() {
        window.removeEventListener('resize', deferredUpdate);
        window.removeEventListener('orientationchange', deferredUpdate);
        window.removeEventListener('scroll', deferredUpdate);
        document.removeEventListener('touchmove', deferredUpdate);

        // loop again
        loopId = loop(update);
    }

    // Loop
    function update() {
        if (setPosition() && pause === false) {
            animate();
            // loop again
            loopId = loop(update);
        } else {
            loopId = null;
            // Don't animate until we get a position updating event
            window.addEventListener('resize', deferredUpdate);
            window.addEventListener('orientationchange', deferredUpdate);
            window.addEventListener('scroll', deferredUpdate, supportsPassive ? { passive: true } : false);
            document.addEventListener('touchmove', deferredUpdate, supportsPassive ? { passive: true } : false);
        }
    }

    // Transform on parallax element
    function animate() {
        for (var i = 0; i < elems.length; i++) {
            var block = blocks[i];

            if (block.paused)
                continue;

            speed = block.speed;

            if (block.type === 'bg') {
                // set bg parallax offset
                var ypos = Math.round((block.top - scrollTop) * speed) + (block.offset * -1);
                elems[i].style['background-position'] = 'center ' + ypos + "px";
            }
            else if (block.type === 'content') {
                var rate = 100 / (block.bottom + winHeight - block.top) * ((scrollTop + winHeight) - block.top);
                rate = rate * block.speedRatio;
                var ytransform = (rate - 50) * (speed * -6) + block.offset;

                elems[i].style[transformProp] = 'translate3d(0, ' + ytransform + 'px, 0)' + block.transform;
            }
        }
    }

    var Parallax = SmartStore.parallax = {
        init: function (options /*{ context: Element, selector: string}*/) {
            var opts = options || {};
            var ctx = $(opts.context || document.body);
            var selector = opts.selector || '.parallax';

            if (!initialized) {
                elems = ctx.find(selector).toArray();
                initialize();
                initialized = true;
            }
        }
    };

})(jQuery, window, document);
