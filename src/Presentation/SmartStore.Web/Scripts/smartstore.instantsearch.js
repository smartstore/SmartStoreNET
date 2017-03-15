(function ($, window, document, undefined) {

    // on document ready
	$(function () {

		var _baseHighlighter = $.fn.typeahead.Constructor.prototype.highlighter;

		var searchBox = $('#instantsearch');
		if (searchBox.length == 0)
			return;

		var searchResult,
			minLength = searchBox.data("minlength"),
			//showThumbs = searchBox.data("showthumbs"),
			url = searchBox.data("url");

		searchBox.typeahead({
			items: 16,
			minLength: minLength,
			keyUpDelay: 400,
			//menu: '<ul class="typeahead dropdown-menu dropdown-instantsearch{0}"></ul>'.format(showThumbs ? " rich" : ""),
			menu: '<ul class="typeahead dropdown-menu dropdown-instantsearch"></ul>',
			autoSelectFirstItem: false,
			source: function (query, process) {
			    var spinner = $('#instantsearch-progress');
			    if (spinner.length === 0) {
			        spinner = createCircularSpinner(20).attr('id', 'instantsearch-progress').appendTo(searchBox.parent());
			    }
			    spinner.addClass('active');

				return $.ajax({
					dataType: "json",
					url: url,
					data: { term: query },
					type: 'GET',
					success: function (json) {
						searchResult = json;
						//var items = $.map(json, function (val, i) {
						//	return val.label;
						//});
						process(json);
					},
					error: function () {
						searchResult = null;
					},
					complete: function () {
					    spinner.removeClass('active');
					}
				});
			},
			updater: function (label) {
				if (!searchResult)
					return;

				//var item = _.find(searchResult, function (x) { return x.label == label });
				//setLocation(item.producturl);
				searchBox.val(label);
				searchBox.closest("form").submit();
			},
			matcher: function (item) {
				// items are filtered already. Do not filter again!
				return true;
			},
			highlighter: function (label) {
				var inner = _baseHighlighter.call(this, label);
				//if (!showThumbs) {
				//	return inner;
				//}

				//var item = _.find(searchResult, function (x) { return x.label == label });

				var html =
					"<div class='item-wrapper'>"
					+ "<div class='item-labels'>"
					+ "<div class='item-primary text-overflow'>" + inner + "</div>"
					+ "</div>"
					+ "</div>";

				//var html = ''
				//	+ "<div class='item-wrapper'>"
				//	+ "<div class='item-thumb'><img src='" + item.productpictureurl + "' /></div>"
				//	+ "<div class='item-labels'>"
				//	+ "<div class='item-primary text-overflow'>" + inner + "</div>"
				//	+ "<div class='item-secondary text-overflow'>" + item.secondary + "</div>"
				//	+ "</div>"
				//	+ "</div>";

				return html;
			}
		});

		searchBox.closest("form").on("submit", function (e) {
			if (!searchBox.val()) {
				var frm = $(this);
				var shakeOpts = { direction: "right", distance: 4, times: 2 };
				frm.stop(true, true).effect("shake", shakeOpts, 400, function () {
					searchBox.trigger("focus").removeClass("placeholder")
				});
				return false;
			}

			return true;
		});
    });

})( jQuery, this, document );

