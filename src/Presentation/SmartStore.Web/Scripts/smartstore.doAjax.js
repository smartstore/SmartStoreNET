/*
*  Project: SmartStore ajax wrapper
*  Author: Marcus Gesing, SmartStore AG
*/

; (function ($, window, document, undefined) {

	$.fn.doAjax = function (options) {
		normalizeOptions(this, options);

		if (_.isEmpty(options.url)) {
			console.log('doAjax can\'t find the url!');
		}
		else if (!_.isFalse(options.valid)) {
			doRequestSwitch(options);
		}

		return this.each(function () { });
	};

	$.fn.doAjax.defaults = {
	    /* [...] */
	};


	function normalizeOptions(element, opt) {
		opt.type = (_.isUndefined(opt.type) ? 'POST' : opt.type);
		opt.ask = (_.isUndefined(opt.ask) ? $(element).attr('data-ask') : opt.ask);

		//if (opt.type === 'POST' && $(element).is('form')) {
        if ($(element).is('form')) {
			if (_.isUndefined(opt.data))
				opt.data = $(element).serialize();
			if (_.isUndefined(opt.url))
			    opt.url = $(element).attr('action');
		}

		opt.url = (_.isUndefined(opt.url) ? findUrl(element) : opt.url);
	}

	function findUrl(element) {
		var url;
		if (_.isObject(element)) {
			url = $(element).attr('href');
			if (typeof url === 'string' && url.substr(0, 11) === 'javascript:')
				url = '';

			if (_.isUndefined(url) || url.length <= 0)
				url = $(element).attr('data-url');

			if (_.isUndefined(url) || url.length <= 0)
				url = $(element).attr('data-button');
		}
		return url;
	}

	function showAnimation(opt) {
	    if (opt.curtainTitle) {
	        $.throbber.show(opt.curtainTitle);
	    }
	    else if (opt.throbber) {
	        $(opt.throbber).removeData('throbber').throbber({ white: true, small: true });
	    }
	    else if (opt.smallIcon) {
	        $(opt.smallIcon).append('<span class="ajax-loader-small"></span>');
	    }
	}

	function hideAnimation(opt) {
	    if (opt.curtainTitle)
	        $.throbber.hide(true);
	    if (opt.throbber)
	        $(opt.throbber).data('throbber').hide(true);
	    if (opt.smallIcon)
	        $(opt.smallIcon).find('span.ajax-loader-small').remove();
	}

	function doRequest(opt) {
		$.ajax({
			cache: false,
			type: opt.type,
			data: opt.data,
			url: opt.url + (_.isEmpty(opt.appendToUrl) ? '' : opt.appendToUrl),
			async: opt.async,
			beforeSend: function () {
				_.call(opt.callbackBeforeSend);
			},
			success: function (resp) {
				_.call(opt.callbackSuccess, resp);
			},
			error: function (objXml) {
				try {
					if (objXml != null && objXml.responseText != null && objXml.responseText !== '') {
						if (_.isTrue(opt.consoleError))
							console.error(objXml.responseText);
						else
							EventBroker.publish("message", { title: objXml.responseText, type: "error" });
					}
				}
				catch (e) { }
			},
			complete: function () {
				hideAnimation(opt);
				_.call(opt.callbackComplete);
			}
		});

		showAnimation(opt);
	}

	function doRequestSwitch(opt) {
		// TODO: implement overlayable message/confirm box... $.overlayer('confirm', opt.ask, doRequest);
		if (_.isEmpty(opt.ask)) {
			doRequest(opt);
		}
		else if (confirm(opt.ask)) {
			doRequest(opt);
		}
	}


})(jQuery, window, document);