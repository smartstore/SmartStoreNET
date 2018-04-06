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
				var rgTextColor = /^text-(muted|primary|success|danger|warning|info|dark|white)$/;
				var rgTextAlign = /^text-(left|center|right)$/;
				var rgDisplay = /^display-[1-4]$/;
				options.cssclass.classes = {
					"alert alert-primary": { toggle: rgAlert },
					"alert alert-secondary": { toggle: rgAlert },
					"alert alert-success": { toggle: rgAlert },
					"alert alert-danger": { toggle: rgAlert },
					"alert alert-warning": { toggle: rgAlert },
					"alert alert-info": { toggle: rgAlert },
					"alert alert-dark": { toggle: rgAlert },
					"text-muted": { displayClass: "px-2 py-1", inline: true, toggle: rgTextColor },
					"text-primary": { displayClass: "px-2 py-1", inline: true, toggle: rgTextColor },
					"text-success": { displayClass: "px-2 py-1", inline: true, toggle: rgTextColor },
					"text-danger": { displayClass: "px-2 py-1", inline: true, toggle: rgTextColor },
					"text-warning": { displayClass: "px-2 py-1", inline: true, toggle: rgTextColor },
					"text-info": { displayClass: "px-2 py-1", inline: true, toggle: rgTextColor },
					"text-dark": { displayClass: "px-2 py-1", inline: true, toggle: rgTextColor },
					"text-white": { displayClass: "px-2 py-1 bg-gray", inline: true, toggle: rgTextColor  },
					"text-left": { displayClass: "px-2 py-1 border", style: 'border-style: dashed !important', toggle: rgTextAlign },
					"text-center": { displayClass: "px-2 py-1 border", style: 'border-style: dashed !important', toggle: rgTextAlign },
					"text-right": { displayClass: "px-2 py-1 border", style: 'border-style: dashed !important', toggle: rgTextAlign },
					"btn btn-primary": { inline: true, toggle: rgBtn },
					"btn btn-secondary": { inline: true, toggle: rgBtn },
					"btn btn-success": { inline: true, toggle: rgBtn },
					"btn btn-danger": { inline: true, toggle: rgBtn },
					"btn btn-warning": { inline: true, toggle: rgBtn },
					"btn btn-info": { inline: true, toggle: rgBtn },
					"btn btn-dark": { inline: true, toggle: rgBtn },
					"rounded": { displayClass: "px-2 py-1 bg-light border rounded", toggle: /^rounded(-.+)?$/ },
					"rounded-0": { displayClass: "px-2 py-1 bg-light border", toggle: /^rounded(-.+)?$/ },
					"list-unstyled": { },
					"display-1": { displayClass: "fs-h1", toggle: rgDisplay },
					"display-2": { displayClass: "fs-h2", toggle: rgDisplay },
					"display-3": { displayClass: "fs-h3", toggle: rgDisplay },
					"display-4": { displayClass: "fs-h4", toggle: rgDisplay },
					"lead": { },
					"jumbotron": { displayClass: "p-4 fs-h3 font-weight-400" },
				};
			}

			addStyleString(".scrollable-menu {height: auto; max-height: 340px; width:360px; overflow-x: hidden; padding:0 !important}");

			context.memo('button.cssclass', function () {
				return ui.buttonGroup([
					ui.button({
						className: 'dropdown-toggle',
						contents: ui.icon("fa fa-css3"),
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

							applyClassToSelection(value, obj);
						}
					})
				]).render();
				return $optionList;
			});

			function applyClassToSelection(value, obj) {
				var controlNode = $(context.invoke("restoreTarget"));
				var sel = window.getSelection();
				var node = $(sel.focusNode.parentElement, ".note-editable");
				var currentNodeIsInline = isInlineElement(node[0]);
				var caret = sel.type == 'None' || sel.type == 'Caret';

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
			}

			function addStyleString(str) {
				var node = document.createElement('style');
				node.innerHTML = str;
				document.body.appendChild(node);
			}

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

			// This method will be called when editor is initialized by $('..').summernote();
			// You can create elements for plugin
			this.initialize = function () {

			};

			// This methods will be called when editor is destroyed by $('..').summernote('destroy');
			// You should remove elements on `initialize`.
			this.destroy = function () {
                /*  this.$panel.remove();
                 this.$panel = null; */
			};
		}
	});
}));