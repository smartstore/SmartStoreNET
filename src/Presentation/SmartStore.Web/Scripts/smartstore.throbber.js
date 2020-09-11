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

        show: function (o) {
            if (this.visible)
                return;

            var self = this,
                opts = $.extend({}, this.options, o);

            // create throbber if not avail
            if (!self.throbber) {
                self.throbber = $('<div class="throbber"><div class="throbber-flex"><div><div class="throbber-content"></div></div></div></div>')
                    .addClass(opts.cssClass)
                    .addClass(opts.small ? "small" : "large")
                    .appendTo(opts._global ? 'body' : self.el);
                if (opts.white) {
                    self.throbber.addClass("white");
                }
                if (opts._global) {
                    self.throbber.addClass("global");
                }
                else {
                    if (/static/.test(self.el.css("position"))) {
                        self.el.css("position", "relative");
                    }
                }

                self.throbberContent = self.throbber.find(".throbber-content");
                var spinner = window.createCircularSpinner(opts.small ? 50 : 100, true, 3);
                spinner.insertAfter(self.throbberContent);

                self.initialized = true;
            }

            // set text
            self.throbber.css({ visibility: 'hidden', display: 'block' });
            self.throbberContent.html(opts.message);
            self.throbberContent.toggleClass('hide', !(_.isString(opts.message) && opts.message.trim().length > 0));
            self.throbber.css({ visibility: 'visible', opacity: 0 });

            var show = function () {
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
            self.throbber.delay(opts.delay).animate({ opacity: 1 }, opts.speed || 0, "linear", show);

            if (opts.timeout) {
                var hideFn = _.bind(self.hide, this);
                window.setTimeout(hideFn, opts.timeout + opts.delay);
            }

        },

        hide: function (immediately) {
            var self = this, opts = this.options;
            if (self.throbber && self.visible) {
                var hide = function () {
                    self.throbber.css('display', 'none');
                }
                self.visible = false;

                !defaults.speed || _.isTrue(immediately)
                    ? self.throbber.stop(true).hide(0, hide)
                    : self.throbber.stop(true).animate({ opacity: 0 }, opts.speed || 0, "linear", hide);
            }

        }

    }

    // A really lightweight plugin wrapper around the constructor, 
    // preventing against multiple instantiations
    $.fn[pluginName] = function (options) {
        return this.each(function () {
            if (!$.data(this, pluginName)) {
                options = $.extend({}, $[pluginName].defaults, options);
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

    $[pluginName] = {

        // the global, default plugin options
        defaults: defaults,

        // options: a message string || options object
        show: function (options) {
            var opts = $.extend(defaults, _.isString(options) ? { message: options } : options, { show: false, _global: true });

            if (!globalThrobber) {
                globalThrobber = $(window).throbber(opts).data("throbber");
            }

            globalThrobber.show(opts);
        },

        hide: function (immediately) {
            if (globalThrobber) {
                globalThrobber.hide(immediately);
            }
        }

    } // $.throbber

})(jQuery, window, document);

