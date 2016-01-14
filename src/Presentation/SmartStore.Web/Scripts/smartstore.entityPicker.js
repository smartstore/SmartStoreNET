/*
*  Project: SmartStore entity picker
*  Author: Marcus Gesing, SmartStore AG
*/

; (function ($, window, document, undefined) {

	$.fn.entityPicker = function (options) {

		options = normalizeOptions(this, options || {});
		loadDialog(options);

		return this.each(function () { });
	};


	function normalizeOptions(element, opt) {
		if (_.isEmpty(opt.entity)) {
			opt.entity = 'product';
		}

		if (_.isEmpty(opt.url)) {
			opt.url = $(element).attr('data-url');
		}

		if (_.isEmpty(opt.url)) {
			console.log('entityPicker cannot find the url for entity picker!');
		}

		return opt;
	}

	function loadDialog(opt) {
		var dialog = $('#entity-picker-' + opt.entity + '-dialog');

		function showAndFocusDialog() {
			dialog.modal('show');
			setTimeout(function () {
				dialog.find('.modal-body :input:visible:enabled:first').focus();
			}, 1000);
		}

		if (dialog.length) {
			showAndFocusDialog();
		}
		else {
			$.ajax({
				cache: false,
				type: 'GET',
				data: { "Entity": opt.entity },
				url: opt.url,
				success: function (response) {
					$('body').append(response);
					dialog = $('#entity-picker-' + opt.entity + '-dialog');
					dialog.find('.caption').html(opt.caption || '&nbsp;');
					showAndFocusDialog();
				},
				error: function (objXml) {
					try {
						if (objXml != null && objXml.responseText != null && objXml.responseText !== '') {
							EventBroker.publish("message", { title: objXml.responseText, type: "error" });								
						}
					}
					catch (e) { }
				}
			});
		}
	}

})(jQuery, window, document);