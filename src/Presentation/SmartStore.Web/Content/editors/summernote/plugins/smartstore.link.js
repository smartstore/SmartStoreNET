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
	function isImg(node) {
		return node && node.nodeName.toUpperCase() === "IMG";
	}

	$.extend($.summernote.plugins, {
		'linkDialog': function (context) {
			var self = this,
				ui = $.summernote.ui,
				$body = $(document.body),
				$editor = context.layoutInfo.editor,
				options = context.options,
				lang = options.langInfo,
				buttons = context.modules.buttons,
				editor = context.modules.editor;

			context.memo('button.link', function () {
				return ui.button({
					contents: ui.icon(options.icons.link),
					callback: function (btn) {
						btn.data("placement", "bottom");
						btn.data("trigger", "hover");
						btn.attr("title", lang.link.link + buttons.representShortcut('linkDialog.show'));
						btn.tooltip();
					},
					click: function () {
						self.show();
					}
				}).render();
			});

			// Create custom "unlink image" button for the image popover
			context.memo('button.unlinkImage', function () {
				return ui.button({
					contents: ui.icon(options.icons.unlink),
					className: 'btn-unlink-image',
					callback: function (btn) {
						btn.data("placement", "bottom");
						btn.data("trigger", "hover");
						btn.attr("title", lang.link.unlink);
						btn.tooltip();
					},
					click: function () {
						self.unlinkImage(this);
					}
				}).render();
			});

			this.initialize = function () {
				context.memo('help.linkDialog.show', options.langInfo.help['linkDialog.show']);

				var $container = options.dialogsInBody ? $body : $editor;
				var body = [
					'<div class="form-group note-form-group">',
					'	<label class="note-form-label">URL</label>',
					'	<div class="input-group input-group-sm">',
					'		<input id="note-link-url" class="note-link-url form-control note-form-control note-input" type="text" value="http://" />',
					'		<div class="input-group-append">',
					'			<button class="btn btn-secondary btn-browse" type="button">' + lang.link.browse + '...</button>',
					'		</div>',
					'	</div>',
					'</div>',
					'<div class="form-group note-form-group form-group-text">',
					'	<label class="note-form-label">' + lang.link.textToDisplay + '</label>',
					'	<input class="note-link-text form-control form-control-sm note-form-control note-input" type="text" />',
					'</div>',
					'<div class="form-group note-form-group">',
					'	<label class="note-form-label">' + lang.attrs.cssClass + '</label>',
					'	<input class="note-link-class form-control form-control-sm note-form-control note-input" type="text" />',
					'</div>',
					'<div class="form-group note-form-group">',
					'	<label class="note-form-label">' + lang.attrs.cssStyle + '</label>',
					'	<input class="note-link-style form-control form-control-sm note-form-control note-input" type="text" />',
					'</div>',
					'<div class="form-group note-form-group">',
					'	<label class="note-form-label">' + lang.attrs.rel + ' <small class="text-muted">(alternate, author, help, license, next, nofollow, noreferrer, prefetch, prev,...)</small></label>',
					'	<input class="note-link-rel form-control form-control-sm note-form-control note-input" type="text" />',
					'</div>',
					'<div class="form-check">',
					'	<input id="sn-checkbox-open-in-new-window" class="form-check-input note-new-window" type="checkbox" checked aria-checked="true" />',
					'	<label for="sn-checkbox-open-in-new-window" class="form-check-label">' + lang.link.openInNewWindow + '</label>',
					'</div>'
				].join('');
				var buttonClass = 'btn btn-primary note-btn note-btn-primary note-link-btn';
				var footer = [
					'<button type="button" class="btn btn-secondary btn-flat" data-dismiss="modal">' + Res['Common.Cancel'] + '</button>',
					'<button type="submit" class="' + buttonClass + '" disabled>' + Res['Common.OK'] + '</button>',
				].join('');
				self.$dialog = ui.dialog({
					className: 'link-dialog',
					title: lang.link.link,
					fade: options.dialogsFade,
					body: body,
					footer: footer
				}).render().appendTo($container);

				self.handleUnlinkButtonState();
			};

			// Hack: toggle our custom "unlink image" button when
			// imagePopover is about to be shown.
			this.handleUnlinkButtonState = function () {
				var popover = context.modules.imagePopover;

				// save the original summernote method
				var fnImagePopoverUpdate = popover.update;

				// decorate the original method with our cusrom stuff
				popover.update = function (target) {
					var btn = popover.$popover.find('.btn-unlink-image');
					var isLinkedImage = $(target).is('img') && $(target).parent().is('a');
					// hide/show the unlink button depending on current selection
					btn.toggle(isLinkedImage);

					// Call the original summernote method
					fnImagePopoverUpdate.apply(popover, [target]);
				};
			}

			// Unlinks a linked image from image popover
			this.unlinkImage = function (btn) {
				var img = $(context.layoutInfo.editable.data('target'));
				if (img.is('img') && img.parent().is('a')) {
					img.unwrap();

					// Ensure that SN saves the change
					context.layoutInfo.note.val(context.invoke('code'));
					context.layoutInfo.note.change();

					// Hide the popover
					var popover = context.modules.imagePopover;
					popover.hide();
				}
			}

			this.destroy = function () {
				ui.hideDialog(this.$dialog);
				self.$dialog.remove();
			};

			this.bindEnterKey = function ($btn) {
				self.$dialog.find('.note-input').on('keypress.linkDialog', function (e) {
					if (e.keyCode === 13) {
						e.preventDefault();
						e.stopPropagation();
						$btn.trigger('click');
						return false;
					}
				});
			};

			this.setAttribute = function (a, el, name) {
				var val = el.val();
				if (val)
					a.attr(name, val)
				else
					a.removeAttr(name);
			}

			this.createLinkRange = function (a) {
				var sc = a[0];
				var so = 0;
				var ec = a[0];
				var eo = a[0].childNodes.length;

				// Create range and assign points again.
				// Something is wrong with Summernote's createRange method.
				var rng = editor.createRange(sc, so, ec, eo);
				rng.sc = sc;
				rng.so = so;
				rng.ec = ec;
				rng.eo = eo;

				return rng;
			}

			this.show = function () {
				var linkInfo, a;
				var img = $(context.layoutInfo.editable.data('target'));
				if (img.length) {
					// Hide "text" control
					self.$dialog.find('.form-group-text').hide();

					a = img.parent();
					if (a.is("a")) {
						linkInfo = {
							a: a, // indicates that an existing link should be edited
							img: img,
							range: self.createLinkRange(a),
							url: a.attr('href'),
							cssClass: a.attr("class"),
							cssStyle: a.attr("style"),
							rel: a.attr("rel"),
							isNewWindow: a.attr('target') === '_blank'
						};
					}
				}
				else {
					self.$dialog.find('.form-group-text').show();
				}

				if (!linkInfo) {
					linkInfo = context.invoke('editor.getLinkInfo');
					if (img.length) {
						linkInfo.img = img;
					}
					a = $(self.findLinkInRange(linkInfo.range));
					if (a.length) {
						linkInfo.cssClass = a.attr("class");
						linkInfo.cssStyle = a.attr("style");
						linkInfo.rel = a.attr("rel");
					}
				}

				context.invoke('editor.saveRange');
				self.showLinkDialog(linkInfo).then(function (linkInfo) {
					var enteredUrl = self.$dialog.find('.note-link-url').val();

					if (options.onCreateLink) {
						enteredUrl = options.onCreateLink(enteredUrl);
					}

					context.invoke('editor.restoreRange');

					if (linkInfo.img && !linkInfo.a) {
						// UNlinked image selected
						linkInfo.img.wrap('<a href="' + enteredUrl + '"></a>');
						a = linkInfo.a = linkInfo.img.parent();
						linkInfo.range = self.createLinkRange(a);
					}
					else if (linkInfo.img && linkInfo.a) {
						// linked image selected
						a = linkInfo.a;
						a.attr("href", enteredUrl);
					}
					else {
						// (Un)linked selected text... let SN process the link
						context.invoke('editor.createLink', linkInfo);
					}			

					// add our custom attributes
					if (a.length) {
						self.setAttribute(a, self.$dialog.find('.note-link-class'), 'class');
						self.setAttribute(a, self.$dialog.find('.note-link-style'), 'style');
						self.setAttribute(a, self.$dialog.find('.note-link-rel'), 'rel');
						if (linkInfo.img) {
							if (linkInfo.isNewWindow) {
								a.attr('target', '_blank');
							}
							else {
								a.removeAttr('target');
							}
						}
					}

					if (linkInfo.img) {
						// Ensure that SN saves the change
						context.layoutInfo.note.val(context.invoke('code'));
						context.layoutInfo.note.change();
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
				var $linkText = self.$dialog.find('.note-link-text');
				var $linkUrl = self.$dialog.find('.note-link-url');
				var $linkClass = self.$dialog.find('.note-link-class');
				var $linkStyle = self.$dialog.find('.note-link-style');
				var $linkRel = self.$dialog.find('.note-link-rel');
				var $linkBtn = self.$dialog.find('.note-link-btn');
				var $openInNewWindow = self.$dialog.find('.note-new-window');
				var $fileBrowse = self.$dialog.find('.btn-browse');

				// if no url was given, copy text to url
				if (!linkInfo.url) {
					linkInfo.url = linkInfo.text;
				}
				$linkText.val(linkInfo.text);
				$linkClass.val(linkInfo.cssClass);
				$linkStyle.val(linkInfo.cssStyle);
				$linkRel.val(linkInfo.rel);

				var toggleLinkBtn = function () {
					var enable = $linkUrl.val();
					if (!linkInfo.img) {
						enable = enable && $linkText.val();
					}
					ui.toggleBtn($linkBtn, enable);
				};
				var handleLinkTextUpdate = function () {
					toggleLinkBtn();
					// if linktext was modified by keyup,
					// stop cloning text from linkUrl
					linkInfo.text = $linkText.val();
				};
				var handleLinkUrlUpdate = function () {
					toggleLinkBtn();
					// display same link on `Text to display` input
					// when create a new link
					if (!linkInfo.text) {
						$linkText.val($linkUrl.val());
					}
				};

				$linkText.on('input.linkDialog', handleLinkTextUpdate);
				$linkUrl.on('input.linkDialog', handleLinkUrlUpdate).val(linkInfo.url);

				toggleLinkBtn();

				var isChecked = linkInfo.isNewWindow !== undefined
					? linkInfo.isNewWindow
					: options.linkTargetBlank;
				$openInNewWindow.prop('checked', isChecked);

				return $.Deferred(function (deferred) {
					ui.onDialogShown(self.$dialog, function () {
						context.triggerEvent('dialog.shown');

						$fileBrowse.on('click.linkDialog', function (e) {
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

						if (!Modernizr.touchevents) {
							$linkUrl.trigger('focus');
						}

						self.bindEnterKey($linkBtn);

						$linkBtn.one('click.linkDialog', function (e) {
							e.preventDefault();
							deferred.resolve({
								img: linkInfo.img,
								a: linkInfo.a,
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
						self.$dialog.find('.note-input').off('keypress');
						$linkText.off('input');
						$linkUrl.off('input');
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