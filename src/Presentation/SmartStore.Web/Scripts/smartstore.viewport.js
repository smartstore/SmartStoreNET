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

		/**
         * Breakpoint detection divs
         */
		detectionDivs: {
			'xs': $('<div class="device-xs d-sm-none"></div>'),
			'sm': $('<div class="device-sm d-none d-sm-block d-md-none"></div>'),
			'md': $('<div class="device-md d-none d-md-block d-lg-none"></div>'),
			'lg': $('<div class="device-lg d-none d-lg-block d-xl-none"></div>'),
			'xl': $('<div class="device-xl d-none d-xl-block"></div>')
		},

		/**
		* Append visibility divs after DOM laoded
		*/
		applyDetectionDivs: function () {
			$(document).ready(function () {
				$.each(self.breakpoints, function (alias) {
					self.breakpoints[alias].appendTo('#device-detection');
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
         * Returns true if currently active breakpoint matches the expression
         */
		isAnyActive: function (breakpoints) {
			var found = false;
			$.each(breakpoints, function (index, alias) {
				// Once first breakpoint matches, return true and break out of the loop
				if (self.breakpoints[alias].is(':visible')) {
					found = true;
					return false;
				}
			});
			return found;
		},

		/**
         * Determines whether current breakpoint matches the expression given
         */
		isMatchingExpression: function (str) {

			var expression = internal.splitExpression(str);

			// Get names of all breakpoints
			var breakpointList = Object.keys(self.breakpoints);

			// Get index of sought breakpoint in the list
			var pos = breakpointList.indexOf(expression.breakpointName);

			// Breakpoint found
			if (pos !== -1) {

				var start = 0;
				var end = 0;

				/**
                 * Parsing viewport.is('<=md') we interate from smallest breakpoint ('xs') and end
                 * at 'md' breakpoint, indicated in the expression,
                 * That makes: start = 0, end = 2 (index of 'md' breakpoint)
                 *
                 * Parsing viewport.is('<md') we start at index 'xs' breakpoint, and end at
                 * 'sm' breakpoint, one before 'md'.
                 * Which makes: start = 0, end = 1
                 */
				if (expression.operator == '<') {
					start = 0;
					end = expression.orEqual ? ++pos : pos;
				}
				/**
                 * Parsing viewport.is('>=sm') we interate from breakpoint 'sm' and end at the end
                 * of breakpoint list.
                 * That makes: start = 1, end = undefined
                 *
                 * Parsing viewport.is('>sm') we start at breakpoint 'md' and end at the end of
                 * breakpoint list.
                 * Which makes: start = 2, end = undefined
                 */
				if (expression.operator == '>') {
					start = expression.orEqual ? pos : ++pos;
					end = undefined;
				}

				var acceptedBreakpoints = breakpointList.slice(start, end);

				return internal.isAnyActive(acceptedBreakpoints);

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
         * Breakpoint aliases, listed from smallest to biggest
         */
		breakpoints: null,

		/**
         * Returns true if current breakpoint matches passed alias
         */
		is: function (str) {
			if (internal.isAnExpression(str)) {
				return internal.isMatchingExpression(str);
			}
			return self.breakpoints[str] && self.breakpoints[str].is(':visible');
		},

		/**
         * Initialize breakpoint detection divs
         */
		init: function () {
			self.breakpoints = internal.detectionDivs;
			internal.applyDetectionDivs();
		},

		/**
         * Returns current breakpoint alias
         */
		current: function () {
			var name = 'unrecognized';
			$.each(self.breakpoints, function (alias) {
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

	// Create a placeholder
	$(document).ready(function () {
		$('<div id="device-detection"></div>').appendTo('body');
	});

	self.init();

	return self;

})(jQuery);