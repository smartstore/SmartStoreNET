/**
 * 
 * copyright 2016 creativeprogramming.it di Stefano Gargiulo
 * email: info@creativeprogramming.it
 * accepting tips at https://www.paypal.me/creativedotit 
 * license: MIT
 * 
 */
(function (factory) {
	/* global define */
	if (typeof define === 'function' && define.amd) {
		// AMD. Register as an anonymous module.
		define(['jquery'], factory);
	} else if (typeof module === 'object' && module.exports) {
		// Node/CommonJS
		module.exports = factory(require('jquery'));
	} else {
		// Browser globals
		factory(window.jQuery);
	}
}(function ($) {
	var inlineTags = 'b big i small tt abbr acronym cite code dfn em kbd stron, samp var a bdo br img map object q script span sub sup button input label select textarea'.split(' ');

	function isInlineElement(el) {
		return _.contains(inlineTags, el.tagName.toLowerCase());
	}

	// Extends plugins for adding hello.
	//  - plugin is external module for customizing.
	$.extend($.summernote.plugins, {
		'cssclass': function (context) {
			var self = this,
				ui = $.summernote.ui,
				$body = $(document.body),
				$editor = context.layoutInfo.editor,
				options = context.options,
				lang = options.langInfo,
				buttons = context.modules.buttons,
				editor = context.modules.editor;

			if (typeof options.cssclass === 'undefined') {
				options.cssclass = {};
			}

			if (typeof options.cssclass.classes === 'undefined') {
				var rgAlert = /^alert(-.+)?$/;
				var rgBtn = /^btn(-.+)?$/;
				var rgBg = /^bg-.+$/;
				var rgTextColor = /^text-(muted|primary|success|danger|warning|info|dark|white)$/;
				var rgTextAlign = /^text-(left|center|right)$/;
				var rgDisplay = /^display-[1-4]$/;
				var rgWidth = /^w-(25|50|75|100)$/;
				options.cssclass.classes = {
					"alert alert-primary": { toggle: rgAlert },
					"alert alert-secondary": { toggle: rgAlert },
					"alert alert-success": { toggle: rgAlert },
					"alert alert-danger": { toggle: rgAlert },
					"alert alert-warning": { toggle: rgAlert },
					"alert alert-info": { toggle: rgAlert },
					"alert alert-light": { toggle: rgAlert },
					"alert alert-dark": { toggle: rgAlert },
					"bg-primary": { displayClass: "px-2 py-1 text-white", inline: true, toggle: rgBg },
					"bg-secondary": { displayClass: "px-2 py-1", inline: true, toggle: rgBg },
					"bg-success": { displayClass: "px-2 py-1 text-white", inline: true, toggle: rgBg },
					"bg-danger": { displayClass: "px-2 py-1 text-white", inline: true, toggle: rgBg },
					"bg-warning": { displayClass: "px-2 py-1 text-white", inline: true, toggle: rgBg },
					"bg-info": { displayClass: "px-2 py-1 text-white", inline: true, toggle: rgBg },
					"bg-light": { displayClass: "px-2 py-1", inline: true, toggle: rgBg },
					"bg-dark": { displayClass: "px-2 py-1 text-white", inline: true, toggle: rgBg },
					"bg-white": { displayClass: "px-2 py-1 border", inline: true, toggle: rgBg },
					"rtl": { displayClass: "text-uppercase", inline: true, toggle: /^ltr$/ },
					"ltr": { displayClass: "text-uppercase", inline: true, toggle: /^rtl$/ },
					"text-muted": { inline: true, toggle: rgTextColor },
					"text-primary": {inline: true, toggle: rgTextColor },
					"text-success": {inline: true, toggle: rgTextColor },
					"text-danger": { inline: true, toggle: rgTextColor },
					"text-warning": { inline: true, toggle: rgTextColor },
					"text-info": { inline: true, toggle: rgTextColor },
					"text-dark": { inline: true, toggle: rgTextColor },
					"text-white": { displayClass: "bg-gray", inline: true, toggle: rgTextColor },
					"font-weight-medium": { inline: true },
					"w-25": { displayClass: "px-2 py-1 bg-light border", toggle: rgWidth },
					"w-50": { displayClass: "px-2 py-1 bg-light border", toggle: rgWidth },
					"w-75": { displayClass: "px-2 py-1 bg-light border", toggle: rgWidth },
					"w-100": { displayClass: "px-2 py-1 bg-light border", toggle: rgWidth },
					"btn btn-primary": { inline: true, toggle: rgBtn, predicate: "a" },
					"btn btn-secondary": { inline: true, toggle: rgBtn, predicate: "a" },
					"btn btn-success": { inline: true, toggle: rgBtn, predicate: "a" },
					"btn btn-danger": { inline: true, toggle: rgBtn, predicate: "a" },
					"btn btn-warning": { inline: true, toggle: rgBtn, predicate: "a" },
					"btn btn-info": { inline: true, toggle: rgBtn, predicate: "a" },
					"btn btn-light": { inline: true, toggle: rgBtn, predicate: "a" },
					"btn btn-dark": { inline: true, toggle: rgBtn, predicate: "a" },
					"rounded": { displayClass: "px-2 py-1 bg-light border rounded", toggle: /^rounded(-.+)?$/ },
					"rounded-0": { displayClass: "px-2 py-1 bg-light border", toggle: /^rounded(-.+)?$/ },
					"list-unstyled": { },
					"display-1": { displayClass: "fs-h1", toggle: rgDisplay },
					"display-2": { displayClass: "fs-h2", toggle: rgDisplay },
					"display-3": { displayClass: "fs-h3", toggle: rgDisplay },
					"display-4": { displayClass: "fs-h4", toggle: rgDisplay },
					"lead": { },
					"jumbotron": { displayClass: "p-4 fs-h3 font-weight-400" }
				};
			}

			if (typeof options.cssclass.imageShapes === 'undefined') {
				options.cssclass.imageShapes = {
					"img-fluid": { inline: true },
					"border": { inline: true },
					"rounded": { toggle: /^(rounded(-.+)?)|img-thumbnail$/, inline: true },
					"rounded-circle": { toggle: /^(rounded(-.+)?)|img-thumbnail$/, inline: true  },
					"img-thumbnail": { toggle: /^rounded(-.+)?$/, inline: true },
					"shadow-sm": { toggle: /^(shadow(-.+)?)$/, inline: true },
					"shadow": { toggle: /^(shadow(-.+)?)$/, inline: true },
					"shadow-lg": { toggle: /^(shadow(-.+)?)$/, inline: true }
				};
			}

			context.memo('button.cssclass', function () {
				return ui.buttonGroup({
					className: 'btn-group-cssclass',
					children: [
						ui.button({
							className: 'dropdown-toggle',
							contents: ui.icon("fab fa-css3"),
							callback: function (btn) {
								btn.data("placement", "bottom")
									.data("trigger", 'hover')
									.attr("title", lang.attrs.cssClass)
									.tooltip();
							},
							data: {
								toggle: 'dropdown'
							}
						}),
						ui.dropdown({
							className: 'dropdown-style scrollable-menu',
							items: _.keys(options.cssclass.classes),
							template: function (item) {
								var obj = options.cssclass.classes[item] || {};
								var cssClass = item + (obj.displayClass ? " " + obj.displayClass : "") + " d-block";
								var cssStyle = obj.style ? ' style="{0}"'.format(obj.style) : '';
								return '<span class="{0}" title="{1}"{2}>{3}</span>'.format(cssClass, item, cssStyle, item);
							},
							click: function (e, namespace, value) {
								e.preventDefault();

								var ddi = $(e.target).closest('[data-value]');
								value = value || ddi.data('value');
								var obj = options.cssclass.classes[value] || {};

								self.applyClassToSelection(value, obj);
							}
						})
					]
				}).render();
			});

			// Image shape stuff
			context.memo('button.imageShapes', function () {
				var imageShapes = _.keys(options.cssclass.imageShapes);
				var button = ui.buttonGroup({
					className: 'btn-group-imageshape',
					children: [
						ui.button({
							className: 'dropdown-toggle',
							contents: ui.icon("fab fa-css3 pr-1"),
							callback: function (btn) {
								btn.data("placement", "bottom");
								btn.data("trigger", "hover");
								btn.attr("title", lang.imageShapes.tooltip);
								btn.tooltip();

								btn.on('click', function () {
									self.refreshDropdown($(this).next(), $(context.layoutInfo.editable.data('target')), true);
								});
							},
							data: {
								toggle: 'dropdown'
							}
						}),
						ui.dropdownCheck({
							className: 'dropdown-shape',
							checkClassName: options.icons.menuCheck,
							items: imageShapes,
							template: function (item) {
								var index = $.inArray(item, imageShapes);
								return lang.imageShapes.tooltipShapeOptions[index];
							},
							click: function (e) {
								e.preventDefault();

								var ddi = $(e.target).closest('[data-value]');
								var value = ddi.data('value');
								var obj = options.cssclass.imageShapes[value] || {};
								
								self.applyClassToSelection(value, obj);
							}
						})
					]
				});

				return button.render();
			});

			this.applyClassToSelection = function (value, obj) {
				var controlNode = $(context.invoke("restoreTarget"));
				var sel = window.getSelection();
				var node = $(sel.focusNode.parentElement, ".note-editable");
				var currentNodeIsInline = isInlineElement(node[0]);
				var caret = sel.type === 'None' || sel.type === 'Caret';

				function apply(el) {
					if (el.is('.' + value.replace(' ', '.'))) {
						// "btn btn-info" > ".btn.btn-info"
						// Just remove the same style
						el.removeClass(value);
						if (!el.attr('class')) {
							el.removeAttr('class');
						}

						if (isInlineElement(el[0]) && !el[0].attributes.length) {
							// Unwrap the node when it is inline and no attribute are present
							el.replaceWith(el.html());
						}
					}
					else {
						if (obj.toggle) {
							// Remove equivalent classes first
							var classNames = (el.attr('class') || '').split(' ');
							_.each(classNames, function (name) {
								if (name && name !== value && obj.toggle.test(name)) {
									el.removeClass(name);
								}
							});
						}

						el.toggleClass(value);
					}
				}

				context.invoke("beforeCommand");

				if (controlNode.length) {
					// Most likely IMG is selected
					if (obj.inline) {
						apply(controlNode);
					}
				}
				else {
					if (!obj.inline) {
						// Apply a block-style only to a block-level element
						if (currentNodeIsInline) {
							// Traverse parents until a block-level element is found
							while (node.length && isInlineElement(node[0])) {
								node = node.parent();
							}
						}

						if (node.length && !node.is('.note-editable')) {
							apply(node);
						}
					}
					else if (obj.inline && caret) {
						apply(node);
					}
					else if (sel.rangeCount) {
						var range = sel.getRangeAt(0).cloneRange();
						var span = $('<span class="' + value + '"></span>');
						range.surroundContents(span[0]);
						sel.removeAllRanges();
						sel.addRange(range);
					}
				}

				context.invoke("afterCommand");
			};

			this.refreshDropdown = function (drop, node /* selectedNode */, noBubble) {
				node = node || $(window.getSelection().focusNode, ".note-editable");

				drop.find('> .dropdown-item').each(function () {
					var ddi = $(this),
						curNode = node,
						value = ddi.data('value'),
						//obj = options.cssclass.classes[value] || {},
						expr = '.' + value.replace(' ', '.'),
						match = false;

					while (curNode.length) {
						if (curNode.is(expr)) {
							match = true;
							break;
						}

						if (noBubble) {
							break;
						}

						if (curNode.is('.note-editable')) {
							break;
						}

						curNode = curNode.parent();
					}

					ddi.toggleClass('checked', match);
				});
			};

			// This events will be attached when editor is initialized.
			this.events = {
				// This will be called after modules are initialized.
				'summernote.init': function (we, e) {
					//console.log('summernote initialized', we, e);
				},
				// This will be called when user releases a key on editable.
				'summernote.keyup': function (we, e) {
					//  console.log('summernote keyup', we, e);
				}
			};

			this.initialize = function () {
				$('.note-toolbar', $editor).on('click', '.btn-group-cssclass .dropdown-item', function (e) {
					// Prevent dropdown close
					e.preventDefault();
					e.stopPropagation();

					self.refreshDropdown($(this).parent());
				});

				$('.note-toolbar', $editor).on('mousedown', '.btn-group-cssclass > .btn', function (e) {
					self.refreshDropdown($(this).next());
				});
			};

			this.destroy = function () {
                /*  this.$panel.remove();
                 this.$panel = null; */
			};
		}
	});
}));