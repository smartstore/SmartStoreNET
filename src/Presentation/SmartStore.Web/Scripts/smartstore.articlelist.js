/*
*  Project: SmartStore Article List
*  Author: Murat Cakir, SmartStore AG
*/

; (function ($, window, document, undefined) {

	$('.artlist-grid').on('mouseenter', '.art', function (e) {
		if (Modernizr.touchevents)
			return;

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
			qname = select.data('qname'),
			url = select.data('url'),
			val = select.val();

		url = window.modifyUrl(url, qname, val);

		window.setLocation(url);
	});


	// Carousel handling
	// -------------------------------------------------------------------

})(jQuery, window, document);