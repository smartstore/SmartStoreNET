/*
*  Project: SmartStore Throbber
*  Author: Murat Cakir, SmartStore AG
*/

; (function ($, window, document, undefined) {

    var pluginName = 'throbber';

    // element: the DOM element
    // options: the instance specific options
    function Throbber(element, options) {
        var self = this;

        this.element = element;
        var el = this.el = $(element),
            opts = this.options = options,
            throbber = this.throbber = null,
            throbberContent = this.throbberContent = null;
            
        this.visible = false;

        this.init = function () {
            throbbers.push(self);
            if (opts.show) {
                self.show();
            }
        };

        this.initialized = false;
        this.init();
        
    }

    Throbber.prototype = {
        
        _reposition: function() {
        	var self = this,
				size = {
            		left: (self.el.width() - self.throbberContent.outerWidth()) / 2,
            		top: (self.el.height() - self.throbberContent.outerHeight()) / 2
				}
            self.throbberContent.css(size);
        },

        show: function (o) {
            
            if (this.visible)
                return;

            var self = this, 
                opts = $.extend( { }, this.options, o);
            
            // create throbber if not avail
            if (!self.throbber) {
                self.throbber = $('<div class="throbber"><div class="throbber-overlay"></div><div class="throbber-content"></div></div>')
                                 .addClass(opts.cssClass)
                                 .addClass(opts.small ? "small" : "large")
                                 .appendTo(opts._global ? 'body' : self.el);
                if (opts.white) {
                    self.throbber.addClass("white");
                }
                if (opts._global) {
                    self.throbber.addClass("global");
                }

                self.throbberContent = self.throbber.find(".throbber-content");

                self.initialized = true;
            }

            // set text and reposition
            self.throbber.css({ visibility: 'hidden', display: 'block' });
            self.throbberContent.html(opts.message);
            self._reposition();
            self.throbber.css({ visibility: 'visible', opacity: 0 });

            var show = function() {
                 if (_.isFunction(opts.callback)) {
                     opts.callback.apply(this);
                 }
                 if (!self.visible) {
                    // could have been set to false in 'hide'.
                    // this can happen in very short running processes.
                    self.hide();
                 }
            }
            
            self.visible = true;
        	self.throbber.delay(opts.delay).transition({opacity: 1}, opts.speed || 0, "linear", show);

        	if (opts.timeout) {
        		var hideFn = _.bind(self.hide, this);
        		window.setTimeout(hideFn, opts.timeout + opts.delay);
            }

        },

        hide: function(immediately) {
            var self = this, opts = this.options;
            if (self.throbber && self.visible) {
                var hide = function() {
                	self.throbber.css('display', 'none');
                }
                self.visible = false;

                !defaults.speed || _.isTrue(immediately)
            		? self.throbber.stop(true).hide(0, hide)
                    : self.throbber.stop(true).transition({ opacity: 0 }, opts.speed || 0, "linear", hide);
            }

        }

    }

    // A really lightweight plugin wrapper around the constructor, 
    // preventing against multiple instantiations
    $.fn[pluginName] = function (options) {
    
        return this.each(function () {
            if (!$.data(this, pluginName)) {
                options = $.extend( {}, $[pluginName].defaults, options );
                $.data(this, pluginName, new Throbber(this, options));
            }
        });

    }

    // global (window)-throbber
    var globalThrobber = null,
        throbbers = [],
        defaults = {
            delay: 0,
            speed: 150,
            timeout: 0,
            white: false,
            small: false,
            message: "Please wait...",
            cssClass: null,
            callback: null,
            show: true,
            // internal
            _global: false
        };
    
    // global resize event
    $(window).on('resize.throbber', function() {
        // resize all active/visible throbbers
        $.each(throbbers, function(i, throbber) {
            if (throbber.initialized && throbber.visible) {
                throbber._reposition();
            }
        })
    });

    $[pluginName] = {
        
        // the global, default plugin options
        defaults: defaults,

        // options: a message string || options object
        show: function(options) {
            var opts = $.extend( defaults, _.isString(options) ? { message: options } : options, { show: false, _global: true } );

            if (!globalThrobber) {
                globalThrobber = $(window).throbber(opts).data("throbber");
            }

            globalThrobber.show(opts);
            
        },

        hide: function() {
            if (globalThrobber) {
                globalThrobber.hide();
            }
        }

    } // $.throbber

})(jQuery, window, document);
