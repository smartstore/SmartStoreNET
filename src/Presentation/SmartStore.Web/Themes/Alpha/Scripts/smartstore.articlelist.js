/*
*  Project: SmartStore Article List
*  Author: Murat Cakir, SmartStore AG
*/

; (function ($, window, document, undefined) {

	$('.artlist-style-grid').on('mouseenter', '.art', function (e) {
		console.log("ENTER", e);
	});

	$('.artlist-style-grid').on('mouseleave', '.art', function (e) {
		console.log("LEAVE", e);
	});

})(jQuery, window, document);