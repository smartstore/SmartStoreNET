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
				el.find('.panel-file').addClass('hide');
				el.find('.panel-url').show();
				el.find('.toggle-file').removeClass("disabled");
				el.find('.toggle-url').addClass("disabled");
			}
			else {
				i.removeClass('fa-globe').addClass('fa-upload');
				el.find('.panel-url').hide();
				el.find('.panel-file').removeClass('hide');
				el.find('.toggle-file').addClass("disabled");
				el.find('.toggle-url').removeClass("disabled");
			}

			el.data("use-url", useUrl);
		};

		this.init = function () {
			var elRemove = el.find('.remove-download'),
				elSaveUrl = el.find('.save-download-url');

			//console.log(el);

			// handle panel switcher buttons
			el.find('.toggle-file, .toggle-url').on('click', function (e) {
				e.preventDefault();
				self.togglePanel($(this).hasClass('toggle-url'));
			});

			el.find('.download-url-value').on('input propertychange paste', function (e) {
				var txt = $(this);
				var hasVal = !!(txt.val()) && txt.val() != txt.data('value');
				var btn = txt.parent().find('.save-download-url');
				if (hasVal)
					btn.removeClass('disabled')
				else
					btn.addClass('disabled');

			});

			// Download removal (transient)
			elRemove.on('click', function (e) {
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
			elSaveUrl.on('click', function (e) {
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