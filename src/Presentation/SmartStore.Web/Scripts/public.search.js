(function ($, window, document, undefined) {

	//
	// Instant Search
	// ==========================================================

	$(function () {
		var box = $('#instasearch'),
			spinner = $('#instasearch-progress');

		if (box.length == 0 || box.data('instasearch') === false)
			return;

		var drop = $('#instasearch-drop'),
			logo = $('.shop-logo'),
			dropBody = drop.find('.instasearch-drop-body'),
			minLength = box.data("minlength"),
			url = box.data("url"),
			keyNav = null;

		box.parent().on('click', function (e) {
			e.stopPropagation();
		});

		box.on('focus', function (e) {
			expandBox();
		});

		box.on('keydown', function (e) {
			if (e.which == 13 /* Enter */) {
				if (keyNav && dropBody.find('.key-hover').length > 0) {
					// Do not post form when key navigation is in progress
					e.preventDefault();
				}
			}
		});

		box.on('keyup', function (e) {
			if (e.which == 27 /* ESC */) {
				closeDrop();
			}
		});

		$(document).on('mousedown', function (e) {
			// Close drop on outside click
			if ($(e.target).closest('.instasearch-form').length > 0)
				return;

			shrinkBox();
			closeDrop();
		});

		var debouncedInput = _.debounce(function (e) {
			doSearch(box.val());
		}, 180, false);
		box.on('input propertychange paste', debouncedInput);

		// Sometimes a previous search term finishes request AFTER
		// a subsequent one. We need to skip rendering in this case.
		var lastTerm;

		function doSearch(term) {
			if (term.length < minLength) {
				closeDrop();
				dropBody.html('');
				return;
			}

			if (spinner.length === 0) {
				spinner = createCircularSpinner(20).attr('id', 'instasearch-progress').appendTo(box.parent());
			}
			// Don't show spinner when result is coming fast (< 100 ms.)
			var spinnerTimeout = setTimeout(function () { spinner.addClass('active'); }, 100)

			// save last entered term in a global variable
			lastTerm = term;

			$.ajax({
				dataType: "html",
				url: url,
				data: { q: term },
				type: 'POST',
				//cache: true,
				success: function (html, status, req) {
					if (lastTerm !== term) {
						// This is the result of a previous term. Get out!
						return;
					}

					if (_.str.isBlank(html)) {
						closeDrop();
						dropBody.html('');
					}
					else {
						var markup = $(html);
						var isMultiCol = markup.hasClass('instasearch-row');
						drop.toggleClass('w-100', !isMultiCol);
						dropBody.html(markup);
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

		function expandBox() {
			var logoWidth = logo.width();
			$('body').addClass('search-focused');
			logo.css('margin-left', (logoWidth * -1) + 'px');

			if (!_.str.isBlank(dropBody.text())) {
				logo.one(Prefixer.event.transitionEnd, function (e) {
					openDrop();
				});
			}
		}

		function shrinkBox() {
			$('body').removeClass('search-focused');
			logo.css('margin-left', '');
		}

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


	//
	// Facets
	// ==========================================================

	$(function () {
		var widget = $('#faceted-search');
		if (widget.length === 0)
			return;

		//
		//	Handle facet widget filter events
		// =============================================
		(function () {
			// Handle checkboxes
			widget.on('change', ':input[type=checkbox].facet-control-native', facetControlClickHandler);

			// Handle radio buttons
			widget.on('click', ':input[type=radio].facet-control-native', facetControlClickHandler);

			function facetControlClickHandler(e) {
				var href = $(this).closest('[data-href]').data('href');
				if (href) {
					setLocation(href);
				}
			}

			// Custom ranges (prices, custom numeric attributes etc.)
			widget.on('click', '.btn-custom-range', function (e) {
				var btn = $(this),
					cnt = btn.closest('.facet-range-container'),
					minVal = cnt.find('.facet-range-from').val(),
					maxVal = cnt.find('.facet-range-to').val();

				var expr = minVal.replace(/[^\d\.\-]/g, '') + '~' + maxVal.replace(/[^\d\.\-]/g, '');

				var url = modifyUrl(null, btn.data('qname'), expr.length > 1 ? expr : null);
				setLocation(url);
			});

			// Validate custom range selection
			widget.on('change', 'select.facet-range-from, select.facet-range-to', function (e, recursive) {
				if (recursive)
					return;

				var select = $(this),
					isMin = select.hasClass('facet-range-from'),
					otherSelect = select.closest('.facet-range-container').find('select.facet-range-' + (isMin ? 'to' : 'from')),
					idx = select.find('option:selected').index(),
					otherIdx = otherSelect.find('option:selected').index();

				function validateRangeControls() {
					var newIdx = Math.min($('option', otherSelect).length - 1, Math.max(0, isMin ? idx + 1 : idx - 1));
					if (newIdx == idx) {
						newIdx = 0;
					}

					$('option:eq(' + newIdx + ')', otherSelect).prop('selected', true);
					otherSelect.trigger('change', [ true ]);
				}

				if (idx > 0 && otherIdx > 0 && ((isMin && idx >= otherIdx) || (!isMin && idx <= otherIdx))) {
					validateRangeControls();
				}
			});
		})();


		//
		//	Handle local search
		// =============================================
		(function () {
			widget.on('input propertychange paste', '.facet-local-search-input', function (e) {
				var el = $(this);

				// Retrieve the input field text and reset the count to zero
				var filter = el.val(),
					rg = new RegExp(filter, "i");

				// Loop through the facet items
				el.closest('.facet-body').find('.facet-item').each(function () {
					var item = $(this);

					// If the facet item does not contain the text phrase hide it
					if (filter.length > 0 && item.text().search(rg) < 0) {
						item.hide();
					}
						// Show the facet item if the phrase matches
					else {
						item.show();
					}
				});
			});
		})();


		//
		//	Handle widget responsiveness (offcanvas)
		// =============================================
		(function () {
			var btn = $('.btn-toggle-filter-widget');
			if (btn.length === 0)
				return;

			var viewport = ResponsiveBootstrapToolkit;

			function collapseWidget(afterResize) {
				if (btn.data('offcanvas')) return;

				// create offcanvas wrapper
				var placement = SmartStore.globalization.culture.isRTL ? 'right' : 'left';
				var offcanvas = $('<aside class="offcanvas offcanvas-' + placement + ' offcanvas-overlay" data-overlay="true"><div class="offcanvas-content offcanvas-scrollable"></div></aside>').appendTo('body');

				// handle .offcanvas-closer click
				offcanvas.one('click', '.offcanvas-closer', function (e) {
					offcanvas.offcanvas('hide');
				});

				// put widget into offcanvas wrapper
				widget.appendTo(offcanvas.children().first());

				btn.data('offcanvas', offcanvas)
				   .attr('data-toggle', 'offcanvas')
				   .attr('data-placement', placement)
				   .attr('data-disablescrolling', 'true')
				   .data('target', offcanvas);

				if (!afterResize) {
					// Collapse all groups on initial page load
					widget.find('.facet-toggle:not(.collapsed)').addClass('collapsed');
					widget.find('.facet-body.show').removeClass('show');
				}
			}

			function restoreWidget() {
				if (!btn.data('offcanvas')) return;

				// move widget back to its origin
				var offcanvas = btn.data('offcanvas');
				widget.appendTo($('.faceted-search-container'));
				offcanvas.remove();

				btn.removeData('offcanvas')
				   .removeAttr('data-toggle')
				   .removeAttr('data-placement')
				   .removeAttr('data-disablescrolling')
				   .removeData('target');
			}

			function toggleOffCanvas(afterResize) {
				var breakpoint = '<lg';
				if (viewport.is(breakpoint)) {
					collapseWidget(afterResize);
				}
				else {
					restoreWidget();
				}
			}

			EventBroker.subscribe("page.resized", function (msg, viewport) {
				toggleOffCanvas(true);
			});

			_.delay(toggleOffCanvas, 10);
		})();
	});

})(jQuery, this, document);

