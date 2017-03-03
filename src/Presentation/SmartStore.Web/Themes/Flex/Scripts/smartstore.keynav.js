/*
*  Project: SmartStore KeyNav 
*  Origin: jQuery keySelection (https://github.com/christianvoigt/jquery-key-selection)
*  Author: Copyright (c) 2014 Christian Voigt, customized by Murat Cakir, SmartStore AG
*/

; (function ($, window, document, undefined) {

	var pluginName = "keyNav",
		defaults = {
			exclusiveKeyListener: false,
			scrollToKeyHoverItem: false,
			scrollContainer: "html,body",
			scrollMargin: 10,
			selectionItemSelector: ".key-target",
			selectedItemHoverClass: "key-hover",
			scrollAnimationDuration: 150,
			keyActions: [ // Use any and as many keys you want. available actions: select, up, down
				{ keyCode: 13, action: "select" }, // enter
				{ keyCode: 38, action: "up" }, // up
				{ keyCode: 40, action: "down" }, // down
				//{ keyCode: 37, action: "up" }, // left
				//{ keyCode: 39, action: "down" }, // right
				//{ keyCode: 9, action: "down" }, // tab
				//{ keyCode: 32, action: "select" } // space
			]
		};

	function Plugin(element, options) {
		this.element = element;
		this.options = $.extend({}, defaults, options);
		this._defaults = defaults;
		this._name = pluginName;

		this.initialized = false;
		this.init();
		this.initialized = true;
	}

	Plugin.prototype = {
		keys: { enter: 13, up: 38, down: 40, left: 37, right: 39, tab: 9, space: 32 },

		init: function () {
			var self = this;
			this.keydownHandler = function (e) {
				var noPropagation = false;
				$.each(self.options.keyActions, function (i, keyAction) {
					if (keyAction.keyCode === e.which) {
						switch (keyAction.action) {
							case "up":
								self.up();
								noPropagation = true;
								break;
							case "down":
								self.down();
								noPropagation = true;
								break;
							case "select":
								self.select();
								noPropagation = true;
								break;
						}
						return false; //break out of each
					}
				});

				if (noPropagation && self.options.exclusiveKeyListener) {
					return false;
				}
			};

			$(document).on("keydown", this.keydownHandler);

			this.clickHandler = function () {
				self.select($(this));
			};

			$(this.element).on("click", this.options.selectionItemSelector, this.clickHandler);

		},

		down: function () {
			if (this.stopped)
				return;

			var hoverCls = this.options.selectedItemHoverClass;

			var $items = $(this.element).find(this.options.selectionItemSelector),
			$keyHover = $items.filter("." + hoverCls),
			index = $items.index($keyHover);
			$keyHover.removeClass(hoverCls);

			if ($items.length > index + 1) {
				$keyHover = $($items[index + 1]).addClass(hoverCls);
			} else {
				$keyHover = $($items[0]).addClass(hoverCls);
			}

			if (this.options.scrollToKeyHoverItem) {
				this.scrollTo($keyHover);
			}

			$(this.element).trigger({
				type: "keyNav.keyHover",
				keyHoverElement: $keyHover.get(0)
			});

		},

		up: function () {
			if (this.stopped)
				return;

			var hoverCls = this.options.selectedItemHoverClass;

			var $items = $(this.element).find(this.options.selectionItemSelector),
			$keyHover = $items.filter("." + hoverCls),
			index = $items.index($keyHover);
			$keyHover.removeClass(hoverCls);
			if (index > 0) {
				$keyHover = $($items[index - 1]).addClass(hoverCls);
			} else {
				$keyHover = $($items[$items.length - 1]).addClass(hoverCls);
			}

			if (this.options.scrollToKeyHoverItem) {
				this.scrollTo($keyHover);
			}

			$(this.element).trigger({
				type: "keyNav.hovered",
				keyHoverElement: $keyHover.get(0)
			});
		},

		select: function ($el) {
			if (this.stopped) 
				return;

			var hoverCls = this.options.selectedItemHoverClass;

			var $selected = $(this.element).find(this.options.selectionItemSelector + ".selected");
			$selected.removeClass("selected");
			if ((!$el && $selected.hasClass(hoverCls)) || ($el && $selected.get(0) === $el.get(0))) {
				return;
			}

			if (!$el || !$el.is(this.options.selectionItemSelector)) {
				$selected = $(this.element).find(this.options.selectionItemSelector + "." + hoverCls).addClass("selected");
			} else {
				$(this.element).find(this.options.selectionItemSelector + "." + hoverCls).removeClass(hoverCls);
				$selected = $el.addClass("selected");
				$selected.addClass(hoverCls);
			}

			$(this.element).trigger({
				type: "keyNav.selected",
				selectedElement: $selected.get(0)
			});
		},

		scrollTo: function ($el) {
			$(this.options.scrollContainer).animate({ scrollTop: $el.offset().top - this.options.scrollMargin }, this.options.scrollAnimationDuration);
		},

		start: function () {
			if (!this.stopped) return;
			this.init();
			this.stopped = false;
		},

		stop: function () {
			if (this.stopped) return;

			$(document).off("keydown", this.keydownHandler);
			$(this.element).off("click", this.options.selectionItemSelector, this.clickHandler);
			this.stopped = true;
		},

		destroy: function () {
			this.stop();
			$.data(this.element, pluginName, null);
		}

	};

	$.fn[pluginName] = function (options) {
		return this.each(function () {
			if (!$.data(this, pluginName)) {
				$.data(this, pluginName, new Plugin(this, options));
			}

			if ((typeof options === "string" || options instanceof String) && (/stop|up|down|select|stop|start|destroy/).test(options)) {
				var plugin = $.data(this, pluginName);
				plugin[options].call(plugin);
			}
		});
	};

})(jQuery, window, document);
