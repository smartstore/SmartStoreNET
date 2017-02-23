/*
*  Project: OffCanvas SideBar
*  Author: Murat Cakir, SmartStore AG
*  Date: 28.01.2016
*/

; (function ($, window, document, undefined) {

    var viewport = ResponsiveBootstrapToolkit;

    // OFFCANVAS PUBLIC CLASS DEFINITION
    // ======================================================

    var OffCanvas = function (element, options) {
        var self = this;

        var el = this.el = $(element);
        this.options = $.extend({}, OffCanvas.DEFAULTS, options);
        this.canvas = $(this.options.canvas || '.wrapper');
        this.state = null;

        if (this.options.placement == 'right') {
            this.el.addClass('offcanvas-right');
        }

        if (this.options.fullscreen) {
            this.el.addClass('offcanvas-fullscreen');
        }

        if (this.options.lg) {
        	this.el.addClass('offcanvas-lg');
        }

        if (this.options.disablescrolling) {
            this.options.disableScrolling = this.options.disablescrolling;
            delete this.options.disablescrolling;
        }

        EventBroker.subscribe("page.resized", function (msg, viewport) {
        	if (viewport.is('>sm')) self.hide();
        });

        if (this.options.autohide) {
            $(document).on('click', $.proxy(this.autohide, this));
        }  

        if (this.options.toggle) {
            this.toggle();
        }

    	// Close on pan[left|right]
        var onRight = el.hasClass('offcanvas-right'),
			canPan = el.hasClass('offcanvas-overlay');

        el.hammer({}).on('panstart panend panleft panright', function (e) {
        	var delta = onRight
				? Math.max(0, e.gesture.deltaX)
				: Math.min(0, e.gesture.deltaX);

        	if (e.type.toLowerCase() === 'panstart') {
        		el.css(Prefixer.css('transition'), 'none');
        	}
        	else if (e.type.toLowerCase() === 'panend') {
        		el.css(Prefixer.css('transform'), '').css(Prefixer.css('transition'), '');
        		if (Math.abs(delta) >= 100) {
        			self.hide();
        		}
        	}
        	else {
        		// panleft or panright
        		if (canPan) {
        			el.css(Prefixer.css('transform'), 'translate3d(' + delta + 'px, 0, 0)');
        		}
        	}
        });
    }


    // OFFCANVAS DEFAULT OPTIONS
    // ======================================================

    OffCanvas.DEFAULTS = {
        canvas: '.wrapper',
        toggle: true,
        placement: 'left',
        fullscreen: false,
		overlay: false,
        autohide: true,
        disableScrolling: false,
        blocker: true
    }


    // OFFCANVAS METHODS
    // ======================================================

    OffCanvas.prototype.show = function (fn) {
        if (this.state) return;

        var body = $('body');
        var self = this;

        var startEvent = $.Event('show.sm.offcanvas');
        this.el.trigger(startEvent);
        if (startEvent.isDefaultPrevented()) return;

        this.state = 'slide-in';

        if (this.options.blocker) {
            body.addClass('canvas-blocking');
        }

        if (this.options.disableScrolling) {
            body.addClass('canvas-noscroll');
        }

        if (this.options.overlay) {
        	body.addClass('canvas-overlay');
        }

        body.one("click", ".offcanvas-closer", function (e) {
            self.hide();
        });

        body.addClass('canvas-sliding');
        body.addClass('canvas-sliding-'
            + (this.options.placement == 'right' ? 'left' : 'right')
			+ (this.options.lg ? ' canvas-lg' : '')
            + (this.options.fullscreen ? ' canvas-fullscreen' : ''));

        this.el.addClass("on").one(Prefixer.event.transitionEnd, function (e) {
            if (self.state != 'slide-in') return;
            body.addClass('canvas-slid');
            self.state = 'slid';
            self.el.trigger('shown.sm.offcanvas');
        });
    }

    OffCanvas.prototype.hide = function (fn) { 
        if (this.state !== 'slid') return;

        var self = this;
        var body = $('body');

        var startEvent = $.Event('hide.sm.offcanvas');
        this.el.trigger(startEvent);
        if (startEvent.isDefaultPrevented()) return;

        self.state = 'slide-out';

        body.addClass('canvas-sliding-out');
        body.removeClass('canvas-blocking canvas-noscroll canvas-slid canvas-sliding canvas-sliding-left canvas-sliding-right canvas-lg canvas-fullscreen canvas-overlay');

        this.el.removeClass("on").one(Prefixer.event.transitionEnd, function (e) {
            if (self.state != 'slide-out') return;

            body.removeClass('canvas-sliding-out');
            self.state = null;
            self.el.trigger('hidden.sm.offcanvas');
        });
    }

    OffCanvas.prototype.toggle = function (fn) {
        if (this.state === 'slide-in' || this.state === 'slide-out') return;
        this[this.state === 'slid' ? 'hide' : 'show']();
    }

    OffCanvas.prototype.autohide = function (e) {
        if ($(e.target).closest(this.el).length === 0)
            this.hide();
    }


    // OFFCANVAS PLUGIN DEFINITION
    // ======================================================

    $.fn.offcanvas = function (option) {
        return this.each(function () {
            var self = $(this),
                data = self.data('sm.offcanvas'),
                options = $.extend({}, OffCanvas.DEFAULTS, self.data(), typeof option === 'object' && option);

            if (!data) self.data('sm.offcanvas', (data = new OffCanvas(this, options)));
            if (typeof option === 'string') data[option]();
        })
    }

    $.fn.offcanvas.Constructor = OffCanvas


    // OFFCANVAS DATA API
    // ======================================================

    $(document).on('click.sm.offcanvas.data-api', '[data-toggle=offcanvas]', function (e) {
        var self = $(this);
        var target = self.data('target') || self.attr('href');
        var $canvas = $(target);
        var data = $canvas.data('sm.offcanvas');
        var options = data ? 'toggle' : self.data();

        e.stopPropagation();
        e.preventDefault();

        if (data)
            data.toggle();
        else
        	$canvas.offcanvas(options);

        return false;
    })

})(jQuery, window, document);
