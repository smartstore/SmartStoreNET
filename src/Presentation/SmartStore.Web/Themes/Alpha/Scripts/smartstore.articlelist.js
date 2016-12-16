/*
*  Project: SmartStore Article List
*  Author: Murat Cakir, SmartStore AG
*/

; (function ($, window, document, undefined) {

	$('.artlist-grid').on('mouseenter', '.art', function (e) {
		var art = $(this);
		var drop = art.find('.art-drop');

		if (drop.length > 0) {
			drop.css('bottom', ((drop.outerHeight(true) * -1) + 1) + 'px');
			art.addClass('active');
			// the Drop can be overlayed by succeeding elements otherwise
			art.closest('.artlist').css('z-index', 100);
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

	$('.artlist-actions').on('change', '.artlist-action-select', function (e) {
		var select = $(this),
			qName = select.data('qname'),
			url = select.data('url'),
			val = select.val();

		var url = window.modifyUrl(url, qName, val);

		window.setLocation(url);
	});
})(jQuery, window, document);