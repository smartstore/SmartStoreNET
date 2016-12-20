/*
*  Project: SmartStore Article List
*  Author: Murat Cakir, SmartStore AG
*/

; (function ($, window, document, undefined) {

	$('.artlist-grid').on('mouseenter', '.art', function (e) {
		var art = $(this);
		var list = art.closest('.artlist');

		if (list.parent().hasClass('artlist-carousel')) {
			return;
		}
			
		var drop = art.find('.art-drop');

		if (drop.length > 0) {
			drop.css('bottom', ((drop.outerHeight(true) * -1) + 1) + 'px');
			art.addClass('active');
			// the Drop can be overlayed by succeeding elements otherwise
			list.css('z-index', 100);
		}
	});

	$('.artlist-grid').on('mouseleave', '.art', function (e) {
		var art = $(this);

		if (art.hasClass('active')) {
			art.removeClass('active')
				.find('.art-drop')
				.css('bottom', 0)
				.closest('.artlist')
				.css('z-index', 'initial');
		}
	});


	// Action panels
	// -------------------------------------------------------------------

	$('.artlist-actions').on('change', '.artlist-action-select', function (e) {
		var select = $(this),
			qName = select.data('qname'),
			url = select.data('url'),
			val = select.val();

		var url = window.modifyUrl(url, qName, val);

		window.setLocation(url);
	});


	// Carousel handling
	// TODO: (mc) Implement a simple generic carousel plugin, which can
	// also be used by MegaMenu.
	// -------------------------------------------------------------------

	$(function () {
		if (Modernizr.touch)
			return;

		var carouselLists = $('.artlist-carousel > .artlist-grid');
		carouselLists.each(function (i, el) {
			var list = $(el);
			var arts = list.find('> .art');
			var carousel = list.parent();

			if (carousel.find('> .scroll-button').length == 0) {
				carousel
					.append('<div class="scroll-button scroll-button-prev"><a class="btn btn-secondary btn-scroll btn-scroll-prev" href="#" rel="nofollow"><i class="fa fa-chevron-left"></i></a></div>')
					.append('<div class="scroll-button scroll-button-next"><a class="btn btn-secondary btn-scroll btn-scroll-next" href="#" rel="nofollow"><i class="fa fa-chevron-right"></i></a></div>');
			}

			var prev = carousel.find('> .scroll-button-prev');
			var next = carousel.find('> .scroll-button-next');

			carousel.on('click', '.btn-scroll', function (e) {
				e.preventDefault();
				scrollToNextInvisibleArt($(this).hasClass('btn-scroll-prev'));
			});

			function scrollToNextInvisibleArt(backwards) {
				// Find the first fully visible article item (from left or right, according to 'backwards')
				var firstVisible = findFirstVisibleArt(backwards);

				// Je nach 'backwards': nimm nächsten oder vorherigen Item (dieser ist ja unsichtbar und da soll jetzt hingescrollt werden)
				var nextItem = backwards
					? firstVisible.prev()
					: firstVisible.next();

				if (nextItem.length == 0)
					return;

				// Linke Pos des Items ermitteln
				var leftPos = nextItem.position().left;

				// 30 = offset für Pfeile
				// Wenn 'backwards == true': zur linken Pos des Items scrollen
				var newMarginLeft = (leftPos * -1) + 1 + 0;
				if (!backwards) {
					// Wenn 'backwards == true': zur rechten Pos des Items scrollen
					var rightPos = leftPos + nextItem.outerWidth(true) + 1;
					newMarginLeft = carousel.width() - rightPos - (nextItem[0].nextElementSibling ? 0 : 0);
				}

				newMarginLeft = Math.min(0, newMarginLeft);

				list.css('margin-left', newMarginLeft + 'px').one("transitionend webkitTransitionEnd", function (e) {
					// Führt UI-Aktualisierung NACH Anim-Ende durch (.one(trans...))
					toggleScrollButtons();
				});
			}

			function findFirstVisibleArt(fromLeft) {
				var items = arts;
				if (!fromLeft) {
					// Reverse articles, because we start from right
					items = $($.makeArray(arts).reverse());
				}

				var result;
				var cntWidth = carousel.width();
				var curMarginLeft = parseFloat(list.css('margin-left'));

				function isInView(pos) {
					var realPos = pos + curMarginLeft;
					return realPos >= 0 && realPos < cntWidth;
				}

				items.each(function (i, el) {
					// Iterates over all articles either from left OR right and breaks, 
					// when the left or right edge of the item is in view.
					var art = $(el);
					var leftPos = art.position().left;
					var leftIn = isInView(leftPos);
					if (leftIn) {
						var rightIn = isInView(leftPos + art.outerWidth(true));
						if (rightIn) {
							result = art;
							return false;
						}
					}
				});

				return result;
			}

			function toggleScrollButtons() {
				var realListWidth = 0;
				arts.each(function (i, el) { realListWidth += parseFloat($(this).outerWidth(true)); });

				var curMarginLeft = parseFloat(list.css('margin-left'));

				carousel.removeClass('has-prev has-next');

				if (realListWidth > carousel.width()) {
					// Items doesn't fit: show 'next' scroll button.
					carousel.addClass('has-next');
				}

				if (curMarginLeft < 0) {
					// We scrolled to right: show 'prev' scroll button
					carousel.addClass('has-prev');

					// Did we reach the right end?
					var endReached = list.width() >= realListWidth;
					if (endReached)
						carousel.removeClass('has-next')
					else
						carousel.addClass('has-next');
				}
				else {
					// We're at the beginning: hide 'prev' scroll button
					carousel.removeClass('has-prev');
				}
			}

			EventBroker.subscribe("page.resized", function (msg, viewport) {
				list.css('margin-left', 0);
				toggleScrollButtons();
			});

			toggleScrollButtons();
		});
	});

})(jQuery, window, document);