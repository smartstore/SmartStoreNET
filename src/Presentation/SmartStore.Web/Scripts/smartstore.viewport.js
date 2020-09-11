/*!
 * Responsive Viewport
 * Based on:  Responsive Bootstrap Toolkit (changed by Murat Cakir)
 * Author:    Maciej Gurban
 * License:   MIT
 * Version:   2.5.1 (2015-11-02)
 * Origin:    https://github.com/maciej-gurban/responsive-bootstrap-toolkit
 */
; var ResponsiveBootstrapToolkit = (function ($) {

    // Internal methods
    var internal = {

        breakpoints: {
            xs: 0,
            sm: 576,
            md: 768,
            lg: 992,
            xl: 1200
        },

        resolveBreakpoints: function () {
            $(document).ready(function () {
                $.each(internal.breakpoints, function (alias) {
                    internal.breakpoints[alias] = parseFloat($('html').css('--breakpoint-' + alias));
                });
            });
        },

		/**
         * Determines whether passed string is a parsable expression
         */
        isAnExpression: function (str) {
            return (str.charAt(0) == '<' || str.charAt(0) == '>');
        },

		/**
         * Splits the expression in into <|> [=] alias
         */
        splitExpression: function (str) {

            // Used operator
            var operator = str.charAt(0);
            // Include breakpoint equal to alias?
            var orEqual = (str.charAt(1) == '=') ? true : false;

			/**
             * Index at which breakpoint name starts.
             *
             * For:  >sm, index = 1
             * For: >=sm, index = 2
             */
            var index = 1 + (orEqual ? 1 : 0);

			/**
             * The remaining part of the expression, after the operator, will be treated as the
             * breakpoint name to compare with
             */
            var breakpointName = str.slice(index);

            return {
                operator: operator,
                orEqual: orEqual,
                breakpointName: breakpointName
            };
        },

		/**
         * Determines whether current breakpoint matches the expression given
         */
        isMatchingExpression: function (str) {

            var expression = internal.splitExpression(str);

            // Get names of all breakpoints
            var breakpointList = Object.keys(internal.breakpoints);

            // Get index of sought breakpoint in the list
            var alias = expression.breakpointName;
            var pos = breakpointList.indexOf(alias);

            // Breakpoint found
            if (pos !== -1) {
                var width = window.innerWidth;
                var min, max;

                if (expression.operator === '>') {
                    min = expression.orEqual
                        ? internal.breakpoints[alias]
                        : internal.breakpoints[breakpointList[pos + 1] || 'xl'];
                    max = 999999;
                }
                else {
                    min = 0;
                    max = (expression.orEqual
                        ? internal.breakpoints[breakpointList[pos + 1] || 'xl']
                        : internal.breakpoints[alias]) - 1;
                }

                return width >= min && width < max;
            }
        }

    };

    // Public methods and properties
    var self = {
		/**
         * Determines default debouncing interval of 'changed' method
         */
        interval: 300,

		/**
         * Returns true if current breakpoint matches passed alias
         */
        is: function (str) {
            if (internal.isAnExpression(str)) {
                return internal.isMatchingExpression(str);
            }

            var match = false;
            var breakpoints = Object.keys(internal.breakpoints)
            $.each(breakpoints, function (pos, alias) {
                if (alias == str) {
                    var min = internal.breakpoints[alias];
                    var max = internal.breakpoints[breakpoints[pos + 1]] || 999999;
                    var width = window.innerWidth;

                    if (width >= min && width < max) {
                        match = true;
                        return false;
                    }
                }
            });

            return match;
        },

		/**
         * Initialize breakpoint detection divs
         */
        init: function () {
            internal.resolveBreakpoints();

            // Notify subscribers about page/content width change
            $(function () {
                if (window.EventBroker) {
                    var currentContentWidth = $('#content').width();
                    var currentTier = self.current();
                    $(window).on('resize', function () {
                        var contentWidth = $('#content').width();
                        //console.log(contentWidth, currentContentWidth);
                        if (contentWidth !== currentContentWidth) {
                            currentContentWidth = contentWidth;
                            EventBroker.publish("page.resized", self);

                            var tier = self.current();
                            if (tier !== currentTier) {
                                currentTier = tier;
                                console.debug("Grid tier changed: " + tier);
                                EventBroker.publish("page.gridtierchanged", tier);
                            }
                        }
                    });
                }
            });
        },

		/**
         * Returns current breakpoint alias
         */
        current: function () {
            var name = 'unrecognized';
            $.each(internal.breakpoints, function (alias) {
                if (self.is(alias)) {
                    name = alias;
                }
            });
            return name;
        },

		/*
         * Waits specified number of miliseconds before executing a callback
         */
        changed: function (fn, ms) {
            var timer;
            return function () {
                clearTimeout(timer);
                timer = setTimeout(function () {
                    fn();
                }, ms || self.interval);
            };
        }

    };

    self.init();

    return self;

})(jQuery);