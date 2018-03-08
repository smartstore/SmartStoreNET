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
					'<div class="row form-group note-form-group">',
					'	<label class="note-form-label col-3">' + lang.link.url + '</label>',
					'	<div class="input-group col-12 col-sm-9">',
					'		<input id="note-link-url" class="note-link-url form-control note-form-control note-input" type="text" value="http://" />',
					'		<div class="input-group-append">',
					'			<button class="btn btn-outline-secondary btn-browse" type="button">' + lang.image.selectFromFiles + '...</button>',
					'		</div>',
					'	</div>',
					'</div>',
					'<div class="row form-group note-form-group">',
					'	<label class="note-form-label col-3">' + lang.link.textToDisplay + '</label>',
					'	<div class=" col-12 col-sm-9"><input class="note-link-text form-control note-form-control note-input" type="text" /></div>',
					'</div>',
					'<div class="row form-group note-form-group">',
					'	<label class="note-form-label col-3">' + 'CSS Klasse' + '</label>',
					'	<div class=" col-12 col-sm-9"><input class="note-link-class form-control note-form-control note-input" type="text" /></div>',
					'</div>',
					'<div class="row form-group note-form-group">',
					'	<label class="note-form-label col-3">' + 'CSS Stil' + '</label>',
					'	<div class=" col-12 col-sm-9"><input class="note-link-style form-control note-form-control note-input" type="text" /></div>',
					'</div>',
					'<div class="row form-group note-form-group">',
					'	<label class="note-form-label col-3">' + 'Rel' + '</label>',
					'	<div class=" col-12 col-sm-9"><input class="note-link-rel form-control note-form-control note-input" type="text" /></div>',
					'</div>',
					'<div class="form-group form-check">',
					'	<input id="sn-checkbox-open-in-new-window" class="form-check-input" type="checkbox" checked aria-checked="true" />',
					'	<label for="sn-checkbox-open-in-new-window" class="form-check-label">' + lang.link.openInNewWindow + '</label>',
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
				var linkInfo, a;
				var img = $(context.layoutInfo.editable.data('target'));
				if (img.length) {
					a = img.parent();
					if (a.is("a")) {
						var sc = a[0];
						var so = 0;
						var ec = a[0];
						var eo = a[0].childNodes.length;

						var rng = editor.createRange(sc, so, ec, eo);
						linkInfo = {
							forImage: true,
							range: rng,
							url: a.attr('href'),
							cssClass: a.attr("class"),
							cssStyle: a.attr("style"),
							rel: a.attr("rel")
						};
					}
				}		

				if (!linkInfo) {
					linkInfo = context.invoke('editor.getLinkInfo');
					a = $(self.findLinkInRange(linkInfo.range));
					if (a.length) {
						linkInfo.cssClass = a.attr("class");
						linkInfo.cssStyle = a.attr("style");
						linkInfo.rel = a.attr("rel");
					}
				}			

				context.invoke('editor.saveRange');
				self.showLinkDialog(linkInfo).then(function (linkInfo) {
					//console.log(linkInfo);
					//context.invoke('editor.restoreRange');

					a = $(self.findLinkInRange(linkInfo.range));

					if (linkInfo.forImage) {
						var $linkUrl = self.$dialog.find('.note-link-url');
						a.attr("href", $linkUrl.val());
					}
					else {
						context.invoke('editor.restoreRange');
						context.invoke('editor.createLink', linkInfo);
					}			

					// add our custom attributes
					if (a.length) {
						console.log(a);
						var $linkClass = self.$dialog.find('.note-link-class');
						var $linkStyle = self.$dialog.find('.note-link-style');
						var $linkRel = self.$dialog.find('.note-link-rel');

						if ($linkClass.val()) a.attr("class", $linkClass.val());
						if ($linkStyle.val()) a.attr("style", $linkStyle.val());
						if ($linkRel.val()) a.attr("rel", $linkRel.val());

						console.log(a);
					}
				}).fail(function () {
					context.invoke('editor.restoreRange');
				});
			};

			this.findLinkInRange = function (rng) {
				var test = [rng.sc, rng.ec, rng.sc.nextSibling, rng.ec.nextSibling, rng.ec.parentNode, rng.ec.parentNode];

				for (var i = 0; i < test.length; i++) {
					if (test[i]) {
						if ($(test[i]).is("a")) {
							return test[i];
						}
					}
				}
			}

			this.showLinkDialog = function (linkInfo) {
				return $.Deferred(function (deferred) {
					var $linkText = self.$dialog.find('.note-link-text');
					var $linkUrl = self.$dialog.find('.note-link-url');
					var $linkClass = self.$dialog.find('.note-link-class');
					var $linkStyle = self.$dialog.find('.note-link-style');
					var $linkRel = self.$dialog.find('.note-link-rel');
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
						$linkClass.val(linkInfo.cssClass);
						$linkStyle.val(linkInfo.cssStyle);
						$linkRel.val(linkInfo.rel);

						$fileBrowse.click(function (e) {
							e.preventDefault();
							var url = context.$note.data('file-browser-url');
							if (url) {
								var modalId = "modal-browse-files";
								url = modifyUrl(url, "type", "#");
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
								forImage: linkInfo.forImage,
								range: linkInfo.range,
								url: $linkUrl.val(),
								text: $linkText.val(),
								cssClasss: $linkClass.val(),
								cssStyle: $linkStyle.val(),
								rel: $linkRel.val(),
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