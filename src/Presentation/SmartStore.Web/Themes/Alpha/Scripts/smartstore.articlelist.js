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
		}
	});

	$('.artlist-grid').on('mouseleave', '.art', function (e) {
		$(this)
			.removeClass('active')
			.find('.art-drop')
			.css('bottom', 0);
	});

	$('.artlist-actions').on('mouseenter mouseleave mousedown', 'select', function (e) {
		var btn = $(this).parent().find('.btn-artlist-action');
		if (e.type == "mouseenter") {
			btn.addClass('focus');
		}
		else if (e.type == "mousedown") {
			btn.addClass('active').removeClass('focus');
			_.delay(function () {
				$('body').one('mousedown', function (e) { btn.removeClass('active'); });
			}, 50);		
		}
		else {
			btn.removeClass('focus');
		}
	});
})(jQuery, window, document);