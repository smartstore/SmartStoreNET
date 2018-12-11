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
            $('body, .canvas-blocker').on('click', $.proxy(this.autohide, this));
        }  

        if (this.options.toggle) {
            this.toggle();
        }

		// Set up events to properly handle (touch) gestures
        this._makeTouchy();
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


	// OFFCANVAS Internal
	// ======================================================

    OffCanvas.prototype._makeTouchy = function (fn) {
        var self = this;
        var el = this.el;

    	// Move offcanvas on pan[left|right] and close on swipe
    	var onRight = el.hasClass('offcanvas-right'),
			canPan = el.hasClass('offcanvas-overlay'),
			panning = false,
			scrolling = false,
			nodeScrollable = null;

    	function getDelta(g) {
    		return onRight
				? Math.max(0, g.delta.x)
				: Math.min(0, g.delta.x);
    	}

    	function isScrolling(e, g) {
    		if (nodeScrollable == null || nodeScrollable.length == 0)
    			return false;

    		var initialScrollDelta = nodeScrollable.data('initial-scroll-top');
    		if (!_.isNumber(initialScrollDelta))
    			return false;

    		return nodeScrollable.scrollTop() != initialScrollDelta;
    	}

    	function handleMove(e, g) {
    		// when scrolling started, do NOT attempt to pan left/right.
    		if (scrolling || (scrolling = isScrolling(e, g)))
    			return;

    		var delta = getDelta(g);
    		panning = !scrolling && delta != 0;

    		if (panning) {
    			// prevent scrolling during panning
    			e.preventDefault();

    			$(e.currentTarget).css(Prefixer.css('transform'), 'translate3d(' + delta + 'px, 0, 0)');
    		}
    		else {
    			if (nodeScrollable != null && nodeScrollable.length > 0) {
    				if (nodeScrollable.height() >= nodeScrollable[0].scrollHeight) {
    					// Content is NOT scrollable. Don't let iOS Safari scroll the body.
    					e.preventDefault();
    				}
    			}
    			else {
    				// Touch occurs outside of any scrollable element. Again: prevent body scrolling.
    				e.preventDefault();
    			}
    		}
    	}

    	el.on('tapstart', function (e, gesture) {
    		if (canPan) {
    			var tabs = $(e.target).closest('.offcanvas-tabs');
    			if (tabs.length > 0) {
    				var tabsWidth = 0;
    				var cntWidth = el.width();
    				tabs.find('.nav-item').each(function () { tabsWidth += $(this).width(); });
    				if (tabsWidth > cntWidth) {
						// Header tabs width exceed offcanvas width. Let it scroll, don't move offcanvas.
    					scrolling = true;
    					return;
    				}
    			}

    			nodeScrollable = $(e.target).closest('.offcanvas-scrollable');
    			if (nodeScrollable.length > 0) {
    				nodeScrollable.data('initial-scroll-top', nodeScrollable.scrollTop());
    			}

    			$(".select2-hidden-accessible", el).select2("close");

    			el.css(Prefixer.css('transition'), 'none');
    			el.on('tapmove.offcanvas', handleMove);
    		}
    	});

    	el.on('tapend', function (e, gesture) {
    		el.off('tapmove.offcanvas')
				.css(Prefixer.css('transform'), '')
				.css(Prefixer.css('transition'), '');

    		if (!scrolling && Math.abs(getDelta(gesture)) >= 100) {
    			self.hide();
    		}

    		nodeScrollable = null;
    		panning = false;
    		scrolling = false;
    	});
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

        body.one("tapend", ".offcanvas-closer", function (e) {
        	e.preventDefault();
            self.hide();
        });
        
        body.addClass('canvas-sliding canvas-sliding-'
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

        $(".select2-hidden-accessible", this.el).select2("close");

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
        var target = $(e.target);
        if (target.closest(this.el).length === 0 && !target.hasClass("select2-results__option"))
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

    $('.canvas-blocker').on('touchmove', function (e) {
    	e.preventDefault();
    });
})(jQuery, window, document);
