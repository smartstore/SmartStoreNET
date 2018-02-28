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
	$.extend($.summernote.plugins, {
		'linkDialog': function (context) {
			var self = this;
			var ui = $.summernote.ui;
			var $body = $(document.body);
			var $editor = context.layoutInfo.editor;
			var options = context.options;
			var lang = options.langInfo;
			var buttons = context.modules.buttons;
			var editor = context.modules.editor;

			context.memo('button.link', function () {
				return ui.button({
					contents: ui.icon(options.icons.link),
					callback: function (btn) {
						btn.data("placement", "bottom");
						btn.attr("title", lang.link.link + buttons.representShortcut('linkDialog.show'));
						btn.tooltip();
					},
					click: function () {
						self.show();
					}
				}).render();
			});

			this.initialize = function () {
				context.memo('help.linkDialog.show', options.langInfo.help['linkDialog.show']);

				var $container = options.dialogsInBody ? $body : $editor;
				var body = [
					'<div class="form-group note-form-group">',
						'<label class="note-form-label">' + lang.link.textToDisplay + '</label>',
						'<input class="note-link-text form-control note-form-control  note-input" type="text" />',
					'</div>',
					'<div class="form-group note-form-group">',
						'<label class="note-form-label">' + lang.link.url + '</label>',
						'<div class="input-group">',
							'<input id="note-link-url" class="note-link-url form-control note-form-control note-input" type="text" value="http://" />',
							'<div class="input-group-append">',
								'<button class="btn btn-outline-secondary btn-browse" type="button">' + lang.image.selectFromFiles + '...</button>',
							'</div>',
						'</div>',
					'</div>',
					'<div class="form-check">',
						'<input id="sn-checkbox-open-in-new-window" class="form-check-input" type="checkbox" checked aria-checked="true" />',
						'<label for="sn-checkbox-open-in-new-window" class="form-check-label">' + lang.link.openInNewWindow + '</label>',
					'</div>'
				].join('');
				var buttonClass = 'btn btn-primary note-btn note-btn-primary note-link-btn';
				var footer = "<button type=\"submit\" href=\"#\" class=\"" + buttonClass + "\" disabled>" + lang.link.insert + "</button>";
				self.$dialog = ui.dialog({
					className: 'link-dialog',
					title: lang.link.insert,
					fade: options.dialogsFade,
					body: body,
					footer: footer
				}).render().appendTo($container);
			};

			this.destroy = function () {
				ui.hideDialog(this.$dialog);
				self.$dialog.remove();
			};

			this.bindEnterKey = function ($input, $btn) {
				$input.on('keypress', function (event) {
					if (event.keyCode === 13) {
						event.preventDefault();
						$btn.trigger('click');
					}
				});
			};

			this.toggleLinkBtn = function ($linkBtn, $linkText, $linkUrl) {
				ui.toggleBtn($linkBtn, $linkText.val() && $linkUrl.val());
			};

			this.show = function () {
				var linkInfo = context.invoke('editor.getLinkInfo');
				context.invoke('editor.saveRange');
				self.showLinkDialog(linkInfo).then(function (linkInfo) {
					context.invoke('editor.restoreRange');
					context.invoke('editor.createLink', linkInfo);
				}).fail(function () {
					context.invoke('editor.restoreRange');
				});
			};

			this.showLinkDialog = function (linkInfo) {
				return $.Deferred(function (deferred) {
					var $linkText = self.$dialog.find('.note-link-text');
					var $linkUrl = self.$dialog.find('.note-link-url');
					var $linkBtn = self.$dialog.find('.note-link-btn');
					var $openInNewWindow = self.$dialog.find('input[type=checkbox]');
					var $fileBrowse = self.$dialog.find('.btn-browse');
					ui.onDialogShown(self.$dialog, function () {
						context.triggerEvent('dialog.shown');
						// if no url was given, copy text to url
						if (!linkInfo.url) {
							linkInfo.url = linkInfo.text;
						}
						$linkText.val(linkInfo.text);

						$fileBrowse.click(function (e) {
							e.preventDefault();
							var url = context.$note.data('file-browser-url');
							if (url) {
								var modalId = "modal-browse-files";
								url = modifyUrl(url, "field", "note-link-url");
								url = modifyUrl(url, "mid", modalId);
								openPopup({
									id: modalId,
									url: url,
									flex: true,
									large: true,
									backdrop: false
								});
							}
						});

						var handleLinkTextUpdate = function () {
							self.toggleLinkBtn($linkBtn, $linkText, $linkUrl);
							// if linktext was modified by keyup,
							// stop cloning text from linkUrl
							linkInfo.text = $linkText.val();
						};
						$linkText.on('input', handleLinkTextUpdate).on('paste', function () {
							setTimeout(handleLinkTextUpdate, 0);
						});
						var handleLinkUrlUpdate = function () {
							self.toggleLinkBtn($linkBtn, $linkText, $linkUrl);
							// display same link on `Text to display` input
							// when create a new link
							if (!linkInfo.text) {
								$linkText.val($linkUrl.val());
							}
						};
						$linkUrl.on('input', handleLinkUrlUpdate).on('paste', function () {
							setTimeout(handleLinkUrlUpdate, 0);
						}).val(linkInfo.url);
						if (!Modernizr.touchevents) {
							$linkUrl.trigger('focus');
						}
						self.toggleLinkBtn($linkBtn, $linkText, $linkUrl);
						self.bindEnterKey($linkUrl, $linkBtn);
						self.bindEnterKey($linkText, $linkBtn);
						var isChecked = linkInfo.isNewWindow !== undefined
							? linkInfo.isNewWindow : options.linkTargetBlank;
						$openInNewWindow.prop('checked', isChecked);
						$linkBtn.one('click', function (e) {
							e.preventDefault();
							deferred.resolve({
								range: linkInfo.range,
								url: $linkUrl.val(),
								text: $linkText.val(),
								isNewWindow: $openInNewWindow.is(':checked')
							});
							ui.hideDialog(self.$dialog);
						});
					});
					ui.onDialogHidden(self.$dialog, function () {
						// detach events
						$linkText.off('input paste keypress');
						$linkUrl.off('input paste keypress');
						$linkBtn.off('click');
						$fileBrowse.off('click');
						if (deferred.state() === 'pending') {
							deferred.reject();
						}
					});
					ui.showDialog(self.$dialog);
				}).promise();
			};
		}
	});
}));