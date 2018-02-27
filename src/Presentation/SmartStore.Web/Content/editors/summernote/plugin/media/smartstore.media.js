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
			var self = this;
			var ui = $.summernote.ui;
			var $body = $(document.body);
			var $editor = context.layoutInfo.editor;
			var options = context.options;
			var lang = options.langInfo;

			context.memo('button.media', function () {
				return ui.button({
					contents: '<i class="fa fa-picture-o">',
					callback: function (btn) {
						btn.data("placement", "bottom");
						btn.attr("title", lang.image.image);
						btn.tooltip();
					},
					click: function () {
						self.show();
					}
				}).render();
			});

			this.initialize = function () {
				var $container = options.dialogsInBody ? $body : $editor;

				var body = [
					'<div class="form-group note-group-image-url" style="overflow:auto;">',
						'<label class="note-form-label">' + lang.image.url + '</label>',
						'<div class="input-group">',
							'<input id="note-image-url" class="note-image-url form-control note-form-control note-input" type="text" />',
								'<div class="input-group-append">',
									'<button class="btn btn-outline-secondary btn-browse" type="button">' + lang.image.selectFromFiles +  '...</button>',
								'</div>',
							'</div>',
						'</div>'
				].join('');
				var buttonClass = 'btn btn-primary note-btn note-btn-primary note-image-btn';
				var footer = "<button type=\"submit\" href=\"#\" class=\"" + buttonClass + "\" disabled>" + lang.image.insert + "</button>";
				self.$dialog = ui.dialog({
					title: lang.image.insert,
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

			this.show = function () {
				context.invoke('editor.saveRange');
				self.showImageDialog().then(function (data) {
					// [workaround] hide dialog before restore range for IE range focus
					ui.hideDialog(self.$dialog);
					context.invoke('editor.restoreRange');
					if (typeof data === 'string') {
						context.invoke('editor.insertImage', data);
					}
					else {
						context.invoke('editor.insertImagesOrCallback', data);
					}
				}).fail(function () {
					context.invoke('editor.restoreRange');
				});
			};

			this.showImageDialog = function () {
				return $.Deferred(function (deferred) {
					//var $imageInput = self.$dialog.find('.note-image-input');
					var $imageUrl = self.$dialog.find('.note-image-url');
					var $imageBtn = self.$dialog.find('.note-image-btn');
					var $imageBrowse = self.$dialog.find('.btn-browse');
					ui.onDialogShown(self.$dialog, function () {
						context.triggerEvent('dialog.shown');
						//// Cloning imageInput to clear element.
						//$imageInput.replaceWith($imageInput.clone().on('change', function (event) {
						//	deferred.resolve(event.target.files || event.target.value);
						//}).val(''));
						$imageBrowse.click(function (e) {
							e.preventDefault();
							var url = context.$note.data('file-browser-url');
							if (url) {
								var modalId = "modal-browse-files";
								url = modifyUrl(url, "type", "image");
								url = modifyUrl(url, "field", "note-image-url");
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
						ui.toggleBtn($imageBtn, false);
						$imageBtn.click(function (e) {
							e.preventDefault();
							deferred.resolve($imageUrl.val());
						});
						$imageUrl.on('keyup paste change input', function (e) {
							var url = $imageUrl.val();
							ui.toggleBtn($imageBtn, url);
						}).val('');
						if (!Modernizr.touchevents) {
							$imageUrl.trigger('focus');
						}
						self.bindEnterKey($imageUrl, $imageBtn);
					});
					ui.onDialogHidden(self.$dialog, function () {
						//$imageInput.off('change');
						$imageUrl.off('keyup paste change input');
						$imageBtn.off('click');
						$imageBrowse.off('click');
						if (deferred.state() === 'pending') {
							deferred.reject();
						}
					});
					ui.showDialog(self.$dialog);
				});
			};
		}
	});
}));