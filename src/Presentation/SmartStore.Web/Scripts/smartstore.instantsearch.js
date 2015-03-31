(function ($, window, document, undefined) {

    // on document ready
	$(function () {
		var searchBox = $('#instantsearch');
		if (searchBox.length == 0)
			return;

		var searchResult,
			minLength = searchBox.data("minlength"),
			url = searchBox.data("url");

		searchBox.typeahead({
			items: 16,
			minLength: minLength,
			keyUpDelay: 400,
			source: function (query, process) {
				$('#instantsearch-progress').removeClass('hide');
				return $.ajax({
					dataType: "json",
					url: url,
					data: { term: query },
					type: 'GET',
					success: function (json) {
						searchResult = json;
						var items = $.map(json, function (val, i) {
							return val.label;
						});
						process(items);
					},
					error: function () {
						searchResult = null;
					},
					complete: function () {
						$('#instantsearch-progress').addClass('hide');
					}
				});
			},
			updater: function (label) {
				if (!searchResult)
					return;

				var item = _.find(searchResult, function (x) { return x.label == label });
				setLocation(item.producturl);
			},
			matcher: function (item) {
				// items are filtered already. Do not filter again!
				return true;
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

