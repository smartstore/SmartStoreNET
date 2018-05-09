;

// Unobtrusive handler for 'Download' editor template
(function ($, window, document, undefined) {

	var pluginName = 'downloadEditor';

	function DownloadEditor(element, options) {
		var self = this;

		// to access the DOM elem from outside of this constructor
		this.element = element;
		var el = this.el = $(element);

		// 'this.options' ensures we can reference the merged instance options from outside
		var opts = this.options = $.extend({}, options);

		this.togglePanel = function (useUrl) {
			if (useUrl && el.hasClass("minimal"))
				return;

			if (useUrl == el.data("use-url"))
				return;

			var i = el.find('.panel-switcher > .dropdown-toggle > .fa');

			if (useUrl) {
				i.removeClass('fa-upload').addClass('fa-globe');
				el.find('.panel-upload').hide();
				el.find('.panel-url').show();
				el.find('.toggle-file').parent().removeClass("disabled");
				el.find('.toggle-url').parent().addClass("disabled");
			}
			else {
				i.removeClass('fa-globe').addClass('fa-upload');
				el.find('.panel-url').hide();
				el.find('.panel-upload').show();
				el.find('.toggle-file').parent().addClass("disabled");
				el.find('.toggle-url').parent().removeClass("disabled");
			}

			el.data("use-url", useUrl);
		};

		this.init = function () {
			var elHidden = el.find('.hidden'),
				elRemove = el.find('.remove'),
				elSaveUrl = el.find('.save-download-url');

			// handle panel switcher buttons
			el.find('.toggle-file, .toggle-url').on('click', function (e) {
				e.preventDefault();
				self.togglePanel($(this).hasClass('toggle-url'));
			});

			// init file uploader
			el.find('.fileupload').fileupload({
				url: el.data('upload-url'),
				dataType: 'json',

				done: function (e, data) {
					var result = data.result;
					if (result.success) {
						el.replaceWith($(result.html));
					}
				},

				error: function (jqXHR, textStatus, errorThrown) {
					if (errorThrown === 'abort') {
						displayNotification('File Upload has been canceled');
					}
				}
			});

			el.find('.download-url-value').on('input propertychange paste', function (e) {
				var txt = $(this);
				var hasVal = !!(txt.val()) && txt.val() != txt.data('value');
				var btn = txt.next();
				btn.attr('disabled', hasVal ? null : "disabled");

				var i = elSaveUrl.find('.fa');
				i.toggleClass('fa-save', hasVal).toggleClass('fa-check', !hasVal);

			});

			// Download removal (transient)
			elRemove.click(function (e) {
				e.preventDefault();

				$.ajax({
					cache: false,
					type: 'POST',
					url: el.data('delete-url'),
					dataType: 'json',
					success: function (data) {
						if (data.success) {
							// just update editor html (with init state)
							el.replaceWith($(data.html));
						}
					}
				});
			});

			// Save download url
			elSaveUrl.click(function (e) {
				e.preventDefault();

				var url = el.find('.download-url-value').val();
				if (!url)
					return false;

				$.ajax({
					cache: false,
					type: 'POST',
					url: $(this).attr('href'),
					data: { "downloadUrl": url },
					dataType: 'json',
					success: function (data) {
						el.replaceWith($(data.html));
					}
				});
			});
		};

		this.initialized = false;
		this.init();
		this.initialized = true;
	}

	$[pluginName] = { defaults: { } };

	$.fn[pluginName] = function (options) {
		return this.each(function () {
			if (!$.data(this, pluginName)) {
				options = $.extend({}, $[pluginName].defaults, options);
				$.data(this, pluginName, new DownloadEditor(this, options));
			}
		});
	}

})(jQuery, window, document);