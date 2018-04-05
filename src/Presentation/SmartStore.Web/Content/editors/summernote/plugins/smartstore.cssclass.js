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
				options.cssclass.classes = [
					{ value: "btn btn-primary", inline: true },
					{ value: "btn btn-secondary", inline: true },
					{ value: "btn btn-success", inline: true },
					{ value: "btn btn-danger", inline: true },
					{ value: "btn btn-warning", inline: true },
					{ value: "btn btn-info", inline: true },
					{ value: "btn btn-dark", inline: true },
					{ value: "alert alert-primary" },
					{ value: "alert alert-secondary" },
					{ value: "alert alert-success" },
					{ value: "alert alert-danger" },
					{ value: "alert alert-warning" },
					{ value: "alert alert-info" },
					{ value: "alert alert-dark" },
					{ value: "text-muted", displayClass: "px-2 py-1", inline: true },
					{ value: "text-primary", displayClass: "px-2 py-1", inline: true },
					{ value: "text-success", displayClass: "px-2 py-1", inline: true },
					{ value: "text-danger", displayClass: "px-2 py-1", inline: true },
					{ value: "text-warning", displayClass: "px-2 py-1" },
					{ value: "text-info", displayClass: "px-2 py-1", inline: true },
					{ value: "text-dark", displayClass: "px-2 py-1", inline: true },
					{ value: "text-white", displayClass: "px-2 py-1 bg-gray", inline: true  },
					{ value: "text-left", displayClass: "px-2 py-1 border" },
					{ value: "text-center", displayClass: "px-2 py-1 border" },
					{ value: "text-right", displayClass: "px-2 py-1 border" },
					{ value: "rounded", displayClass: "px-2 py-1 border rounded" },
					{ value: "rounded-0", displayClass: "px-2 py-1 border" },
					{ value: "list-unstyled" },
					{ value: "display-1", displayClass: "fs-h1" },
					{ value: "display-2", displayClass: "fs-h2" },
					{ value: "display-3", displayClass: "fs-h3" },
					{ value: "display-4", displayClass: "fs-h4" },
					{ value: "lead" },
					{ value: "jumbotron", displayClass: "p-4 fs-h3 font-weight-400" },
				];
			}
			// ui has renders to build ui elements.
			//  - you can create a button with `ui.button`
			var ui = $.summernote.ui;

			addStyleString(".scrollable-menu {height: auto; max-height: 300px; width:360px; overflow-x: hidden; padding:0 !important}");

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
						items: options.cssclass.classes,
						template: function (item) {
							if (typeof item === 'string') {
								item = { title: item, value: item };
								item.title = item;
								item.value = item;
							}
							else {
								item.title = item.value;
							}

							if (item.inline) {
								item.option = "true";
							}

							var cssClass = item.value + (item.displayClass ? " " + item.displayClass : "") + " d-block";
							return '<span class="{0}" title="{1}">{2}</span>'.format(cssClass, item.title, item.value);
						},
						click: function (e, namespace, value) {
							e.preventDefault();
							var ddi = $(e.target).closest('[data-value]');
							var inline = ddi.data('option');

							value = value || ddi.data('value');

							applyClassToSelection(value, inline);

							//var $node = $(context.invoke("restoreTarget"));

							//console.log("$node", $node);

							//if ($node.length == 0) {
							//	$node = $(document.getSelection().focusNode.parentElement, ".note-editable");
							//}

							//if (typeof options.cssclass !== 'undefined' && typeof options.cssclass.debug !== 'undefined' && options.cssclass.debug) {
							//	console.debug(context.invoke("restoreTarget"), $node, "toggling class: " + value, window.getSelection());
							//}

							//$node.toggleClass(value)
						}
					})
				]).render();
				return $optionList;
			});

			function applyClassToSelection(value, inline) {
				var node = $(context.invoke("restoreTarget"));

				context.invoke("beforeCommand");

				if (node.length && inline) {
					// Most likely IMG is selected
					node.removeClass(value).addClass(value);
				}
				else {
					var sel = window.getSelection();
					node = $(sel.focusNode.parentElement, ".note-editable");
					var currentNodeIsInline = isInlineElement(node[0]);

					if (!inline) {
						if (currentNodeIsInline) {
							// Traverse parents until a block-level element is found
							while (node.length && isInlineElement(node[0])) {
								node = node.parent();
							}
						}

						if (node.length && !node.is('.note-editable')) {
							node.removeClass(value).addClass(value);
						}
					}
					else if (inline && currentNodeIsInline) {
						node.removeClass(value).addClass(value);
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