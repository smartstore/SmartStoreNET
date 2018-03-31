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
					//"display-1",
					//"display-2",
					//"display-3",
					//"display-4",
					"lead",
					"rounded",
					"rounded-0",
					"btn btn-primary",
					"btn btn-secondary",
					"btn btn-success",
					"btn btn-danger",
					"btn btn-warning",
					"btn btn-info",
					"text-muted",
					"text-primary",
					"text-warning",
					"text-danger",
					"text-success",
					"alert alert-primary",
					"alert alert-success",
					"alert alert-danger",
					"alert alert-warning",
					"alert alert-info",
					"text-left",
					"text-center",
					"text-right",
					"list-unstyled",
					"jumbotron"];
			}
			// ui has renders to build ui elements.
			//  - you can create a button with `ui.button`
			var ui = $.summernote.ui;

			addStyleString(".scrollable-menu {height: auto; max-height: 300px; max-width:400px; overflow-x: hidden; padding:0 !important}");

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
								item = { tag: "span", title: item, value: item };
							}

							var tag = item.tag;
							var title = item.title;
							var style = item.style ? ' style="' + item.style + '" ' : '';
							var cssClass = item.value || '';

							return '<' + tag + ' ' + style + cssClass + ' class="d-block ' + cssClass + '">' + title + '</' + tag + '>';
						},
						click: function (e, namespace, value) {
							e.preventDefault();
							value = value || $(e.target).closest('[data-value]').data('value');

							var $node = $(context.invoke("restoreTarget"))
							if ($node.length == 0) {
								$node = $(document.getSelection().focusNode.parentElement, ".note-editable");
							}

							if (typeof options.cssclass !== 'undefined' && typeof options.cssclass.debug !== 'undefined' && options.cssclass.debug) {
								console.debug(context.invoke("restoreTarget"), $node, "toggling class: " + value, window.getSelection());
							}

							$node.toggleClass(value)
						}
					})
				]).render();
				return $optionList;
			});

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