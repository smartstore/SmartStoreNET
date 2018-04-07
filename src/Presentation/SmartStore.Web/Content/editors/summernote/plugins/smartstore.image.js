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
		'media': function (context) {
			var self = this,
				ui = $.summernote.ui,
				$body = $(document.body),
				$editor = context.layoutInfo.editor,
				options = context.options,
				lang = options.langInfo,
				buttons = context.modules.buttons,
				editor = context.modules.editor;

			// Image float popover stuff
			context.memo('button.bsFloatLeft', function () {
				return ui.button({
					contents: ui.icon(options.icons.alignLeft),
					callback: function (btn) {
						btn.data("placement", "bottom");
						btn.data("trigger", "hover");
						btn.attr("title", lang.image.floatLeft);
						btn.tooltip();
					},
					click: function (e) {
						var $img = $(context.layoutInfo.editable.data('target'));
						$img.removeClass('float-right float-left').addClass('float-left');
						context.invoke('editor.afterCommand');
					}
				}).render();
			});
			context.memo('button.bsFloatRight', function () {
				return ui.button({
					contents: ui.icon(options.icons.alignRight),
					callback: function (btn) {
						btn.data("placement", "bottom");
						btn.data("trigger", "hover");
						btn.attr("title", lang.image.floatRight);
						btn.tooltip();
					},
					click: function (e) {
						var $img = $(context.layoutInfo.editable.data('target'));
						$img.removeClass('float-right float-left').addClass('float-right');
						context.invoke('editor.afterCommand');
					}
				}).render();
			});
			context.memo('button.bsFloatNone', function () {
				return ui.button({
					contents: ui.icon(options.icons.alignJustify),
					callback: function (btn) {
						btn.data("placement", "bottom");
						btn.data("trigger", "hover");
						btn.attr("title", lang.image.floatNone);
						btn.tooltip();
					},
					click: function (e) {
						var $img = $(context.layoutInfo.editable.data('target'));
						$img.removeClass('float-right float-left');
						context.invoke('editor.afterCommand');
					}
				}).render();
			});

			// ImageDialog replacement with lots of sugar
			context.memo('button.media', function () {
				return ui.button({
					contents: '<i class="fa fa-picture-o">',
					callback: function (btn) {
						btn.data("placement", "bottom");
						btn.data("trigger", "hover");
						btn.attr("title", lang.image.image);
						btn.tooltip();
					},
					click: function () {
						self.show();
					}
				}).render();
			});

			context.memo('button.imageAttributes', function () {
				var button = ui.button({
					contents: '<i class="fa fa-pencil"></i>',
					callback: function (btn) {
						btn.data("placement", "bottom");
						btn.data("trigger", "hover");
						btn.attr("title", lang.image.imageProps);
						btn.tooltip();
					},
					click: function () {
						self.show();
					}
				});
				return button.render();
			});

			this.initialize = function () {
				var $container = options.dialogsInBody ? $body : $editor;

				var body = [
					'<div class="form-group note-group-image-url">',
					'	<label class="note-form-label">' + lang.image.url + '</label>',
					'	<div class="input-group input-group-sm">',
					'		<input id="note-image-src" class="note-image-src form-control note-form-control note-input" type="text" />',
					'		<div class="input-group-append">',
					'			<button class="btn btn-secondary btn-browse" type="button">' + lang.link.browse +  '...</button>',
					'		</div>',
					'	</div>',
					'</div>',
					'<div class="form-group note-form-group form-group-text">',
					'	<label class="note-form-label">Alt</label>',
					'	<input class="note-image-alt form-control form-control-sm note-form-control note-input" type="text" />',
					'</div>',
					'<div class="form-group note-form-group form-group-text">',
					'	<label class="note-form-label">Title</label>',
					'	<input class="note-image-title form-control form-control-sm note-form-control note-input" type="text" />',
					'</div>',
					'<div class="form-group note-form-group">',
					'	<label class="note-form-label">' + lang.attrs.cssClass + '</label>',
					'	<input class="note-image-class form-control form-control-sm note-form-control note-input" type="text" />',
					'</div>',
					'<div class="form-group note-form-group">',
					'	<label class="note-form-label">' + lang.attrs.cssStyle + '</label>',
					'	<input class="note-image-style form-control form-control-sm note-form-control note-input" type="text" />',
					'</div>'
				].join('');
				var footer = [
					'<button type="button" class="btn btn-secondary btn-flat" data-dismiss="modal">' + Res['Common.Cancel'] + '</button>',
					'<button type="submit" class="btn btn-primary note-btn note-btn-primary note-image-btn" disabled>' + Res['Common.OK'] + '</button>',
				].join('');
				self.$dialog = ui.dialog({
					className: 'image-dialog',
					title: lang.image.image,
					fade: options.dialogsFade,
					body: body,
					footer: footer
				}).render().appendTo($container);
			};

			this.destroy = function () {
				ui.hideDialog(this.$dialog);
				self.$dialog.remove();
			};

			this.bindEnterKey = function ($btn) {
				self.$dialog.find('.note-input').on('keypress.imageDialog', function (e) {
					if (e.keyCode === 13) {
						e.preventDefault();
						e.stopPropagation();
						$btn.trigger('click');
						return false;
					}
				});
			};

			this.setAttribute = function (img, el, name) {
				var val = el.val();
				if (val)
					img.attr(name, val)
				else
					img.removeAttr(name);
			}

			this.show = function () {
				var imgInfo = {},
					img = $(context.layoutInfo.editable.data('target'));

				if (img.length) {
					imgInfo = {
						img: img,
						src: img.attr('src'),
						alt: img.attr("alt"),
						title: img.attr("title"),
						cssClass: img.attr("class"),
						cssStyle: img.attr("style"),
					}
				}

				context.invoke('editor.saveRange');
				self.showImageDialog(imgInfo).then(function (imgInfo) {
					// [workaround] hide dialog before restore range for IE range focus
					ui.hideDialog(self.$dialog);
					context.invoke('editor.restoreRange');

					function setAttrs(img, withSrc) {
						if (withSrc) {
							self.setAttribute(img, self.$dialog.find('.note-image-src'), 'src');
						}
						
						self.setAttribute(img, self.$dialog.find('.note-image-alt'), 'alt');
						self.setAttribute(img, self.$dialog.find('.note-image-title'), 'title');
						self.setAttribute(img, self.$dialog.find('.note-image-class'), 'class');
						self.setAttribute(img, self.$dialog.find('.note-image-style'), 'style');
					}

					if (!imgInfo.img) {
						// Insert mode
						context.invoke('editor.insertImage', self.$dialog.find('.note-image-src').val(), setAttrs);
					}
					else {
						// edit mode
						setAttrs(imgInfo.img, true);

						// Ensure that SN saves the change
						context.layoutInfo.note.val(context.invoke('code'));
						context.layoutInfo.note.change();
					}
				}).fail(function () {
					context.invoke('editor.restoreRange');
				});
			};

			this.showImageDialog = function (imgInfo) {
				var $imageUrl = self.$dialog.find('.note-image-src');
				var $imageClass = self.$dialog.find('.note-image-class');
				var $imageStyle = self.$dialog.find('.note-image-style');
				var $imageAlt = self.$dialog.find('.note-image-alt');
				var $imageTitle = self.$dialog.find('.note-image-title');
				var $imageBtn = self.$dialog.find('.note-image-btn');
				var $imageBrowse = self.$dialog.find('.btn-browse');
				
				$imageUrl.on('input.imageDialog', function (e) {
					ui.toggleBtn($imageBtn, $imageUrl.val());
				});

				$imageUrl.val(imgInfo.src);
				$imageClass.val(imgInfo.cssClass);
				$imageStyle.val(imgInfo.cssStyle);
				$imageAlt.val(imgInfo.alt);
				$imageTitle.val(imgInfo.title);

				ui.toggleBtn($imageBtn, imgInfo.src);

				return $.Deferred(function (deferred) {
					ui.onDialogShown(self.$dialog, function () {
						context.triggerEvent('dialog.shown');

						$imageBrowse.on('click.imageDialog', function (e) {
							e.preventDefault();
							var url = context.$note.data('file-browser-url');
							if (url) {
								var modalId = "modal-browse-files";
								url = modifyUrl(url, "type", "image");
								url = modifyUrl(url, "field", "note-image-src");
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
							$imageUrl.trigger('focus');
						}

						self.bindEnterKey($imageBtn);

						$imageBtn.one('click', function (e) {
							e.preventDefault();
							deferred.resolve({
								img: imgInfo.img,
								src: imgInfo.src,
								alt: imgInfo.alt,
								title: imgInfo.title,
								cssClass: imgInfo.cssClass,
								cssStyle: imgInfo.cssStyle
							});
							ui.hideDialog(self.$dialog);
						});
					});

					ui.onDialogHidden(self.$dialog, function () {
						self.$dialog.find('.note-input').off('keypress');
						$imageUrl.off('input');
						$imageBtn.off('click');
						$imageBrowse.off('click');
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