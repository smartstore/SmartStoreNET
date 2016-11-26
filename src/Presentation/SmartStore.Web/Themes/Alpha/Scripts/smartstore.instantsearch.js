(function ($, window, document, undefined) {
	$(function () {
		var box = $('#instasearch');
		if (box.length == 0)
			return;

		var drop = $('#instasearch-drop'),
			dropBody = drop.find('.instasearch-drop-body'),
			minLength = box.data("minlength"),
			url = box.data("url"),
			keyNav = null;

		box.on('input propertychange paste', function (e) {
			var term = box.val();
			doSearch(term);
		});

		box.parent().on('click', function (e) {
			e.stopPropagation();
		});

		box.on('focus', function (e) {
			if (!_.str.isBlank(dropBody.text())) {
				openDrop();
			}
		});

		box.on('keydown', function (e) {
			if (e.which == 13 /* Enter */) {
				if (keyNav && dropBody.find('.key-hover').length > 0) {
					// Do not post form when key navigation is in progress
					e.preventDefault();
				}
			}
		});

		$(document).on('click', function (e) {
			// Close drop on outside click
			closeDrop();
		});

		function openDrop() {
			if (!drop.hasClass('open')) {
				drop.addClass('open');
				beginKeyEvents();
			}
		}
		
		function closeDrop() {
			drop.removeClass('open');
			endKeyEvents();
		}

		function doSearch(term) {
			if (term.length < minLength) {
				closeDrop();
				dropBody.html('');
				return;
			}

			var spinner = $('#instasearch-progress');
			if (spinner.length === 0) {
				spinner = createCircularSpinner(20).attr('id', 'instasearch-progress').appendTo(box.parent());
			}
			// Don't show spinner when result is coming fast (< 100 ms.)
			var spinnerTimeout = setTimeout(function () { spinner.addClass('active'); }, 100)
			
			$.ajax({
				dataType: "html",
				url: url,
				data: { q: term },
				type: 'POST',
				success: function (html) {
					if (_.str.isBlank(html)) {
						closeDrop();
						dropBody.html('');
					}
					else {
						dropBody.html(html);
						openDrop();
					}
				},
				error: function () {
					closeDrop();
					dropBody.html('');
				},
				complete: function () {
					clearTimeout(spinnerTimeout);
					spinner.removeClass('active');
				}
			});
		}

		function beginKeyEvents() {
			if (keyNav)
				return;

			// start listening to Down, Up and Enter keys

			dropBody.keyNav({
				exclusiveKeyListener: false,
				scrollToKeyHoverItem: false,
				selectionItemSelector: ".instasearch-hit",
				selectedItemHoverClass: "key-hover",
				keyActions: [
					{ keyCode: 13, action: "select" }, //enter
					{ keyCode: 38, action: "up" }, //up
					{ keyCode: 40, action: "down" }, //down
				]
			});

			keyNav = dropBody.data("keyNav");

			dropBody.on("keyNav.selected", function (e) {
				// Triggered when user presses Enter after navigating to a hit with keyboard
				var el = $(e.selectedElement);
				var href = el.attr('href') || el.data('href');
				if (href) {
					closeDrop();
					location.replace(href);
				}
			});
		}

		function endKeyEvents() {
			if (keyNav) {
				dropBody.off("keyNav.selected");
				keyNav.destroy();
				keyNav = null;
			}
		}

		box.closest("form").on("submit", function (e) {
			if (!box.val()) {
				// Shake the form on submit but no term has been entered
				var frm = $(this);
				var shakeOpts = { direction: "right", distance: 4, times: 2 };
				frm.stop(true, true).effect("shake", shakeOpts, 400, function () {
					box.trigger("focus").removeClass("placeholder")
				});
				return false;
			}

			return true;
		});
	});
})(jQuery, this, document);

