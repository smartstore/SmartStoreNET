(function ($, window, document, undefined) {
	$(function () {
		var box = $('#instasearch');
		if (box.length == 0)
			return;

		var searchResult,
			drop = $('#instasearch-drop'),
			minLength = box.data("minlength"),
			url = box.data("url");

		box.on('input propertychange paste', function (e) {
			var term = box.val();
			doSearch(term);
		});

		box.parent().on('click', function (e) {
			e.stopPropagation();
		});

		box.on('focus', function (e) {
			if (!_.str.isBlank(drop.text())) {
				drop.addClass('open');
			}
		});

		$(document).on('click', function (e) {
			drop.removeClass('open');
		});

		function doSearch(term) {
			if (term.length < minLength) {
				drop.removeClass('open').html('');
				return;
			}

			var spinner = $('#instasearch-progress');
			if (spinner.length === 0) {
				spinner = createCircularSpinner(20).attr('id', 'instasearch-progress').appendTo(box.parent());
			}
			// don't show spinner when result is coming fast (< 100 ms.)
			var spinnerTimeout = setTimeout(function () { spinner.addClass('active'); }, 100)
			
			$.ajax({
				dataType: "html",
				url: url,
				data: { term: term },
				type: 'POST',
				success: function (html) {
					if (_.str.isBlank(html)) {
						drop.removeClass('open').html('');
					}
					else {
						drop.html(html).addClass('open');
					}
				},
				error: function () {
					drop.removeClass('open').html('');
				},
				complete: function () {
					clearTimeout(spinnerTimeout);
					spinner.removeClass('active');
				}
			});
		}

		box.closest("form").on("submit", function (e) {
			if (!box.val()) {
				var frm = $(this);
				var shakeOpts = { direction: "right", distance: 4, times: 2 };
				frm.stop(true, true).effect("shake", shakeOpts, 400, function () {
					box.trigger("focus").removeClass("placeholder")
				});
				return false;
			}

			return true;
		});

		return;

		box.typeahead({
			items: 16,
			minLength: minLength,
			keyUpDelay: 400,
			//menu: '<ul class="typeahead dropdown-menu dropdown-instantsearch{0}"></ul>'.format(showThumbs ? " rich" : ""),
			menu: '<ul class="typeahead dropdown-menu dropdown-instantsearch"></ul>',
			autoSelectFirstItem: false,
			source: function (query, process) {
				var spinner = $('#instantsearch-progress');
				if (spinner.length === 0) {
					spinner = createCircularSpinner(20).attr('id', 'instantsearch-progress').appendTo(box.parent());
				}
				spinner.addClass('active');

				return $.ajax({
					dataType: "json",
					url: url,
					data: { term: query },
					type: 'POST',
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
				box.val(label);
				box.closest("form").submit();
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
	});
})(jQuery, this, document);

