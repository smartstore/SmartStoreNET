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

			if (useUrl) {
				el.find('.panel-upload').hide();
				el.find('.panel-url').show();
				el.find('.toggle-file').parent().removeClass("disabled");
				el.find('.toggle-url').parent().addClass("disabled");
			}
			else {
				el.find('.panel-url').hide();
				el.find('.panel-upload').show();
				el.find('.toggle-file').parent().addClass("disabled");
				el.find('.toggle-url').parent().removeClass("disabled");
			}

			el.data("use-url", useUrl);
		};

		this.init = function () {
			var elHidden = el.find('.hidden'),
				elRemove = el.find('.remove');

			// handle panel switcher buttons
			el.find('.toggle-file').on('click', function (e) { e.preventDefault(); self.togglePanel(false); });
			el.find('.toggle-url').on('click', function (e) { e.preventDefault(); self.togglePanel(true); });

			// init file uploader
			el.find('.fileupload').fileupload({
				url: el.data('upload-url'),
				dataType: 'json',

				done: function (e, data) {
					var result = data.result;
					if (result.success) {
						// >>>>>> $('#@(clientId + "downloadurl")').html('<a href="' + result.downloadUrl + '">@T("Admin.Download.DownloadUploadedFile"): <strong>' + result.fileName + '</strong></a>');
						elHidden.val(result.downloadId);
						elRemove.show();
					}
				},

				error: function (jqXHR, textStatus, errorThrown) {
					if (errorThrown === 'abort') {
						//alert('File Upload has been canceled');
					}
				}
			});

			// Download removal
			elRemove.click(function (e) {
				// >>>>>> $('#@(clientId + "downloadurl")').html("&nbsp;");
				elHidden.val(0);
				$(this).hide();
				e.preventDefault();
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