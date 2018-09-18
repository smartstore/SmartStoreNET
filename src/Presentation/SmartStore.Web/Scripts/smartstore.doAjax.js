/*
*  Project: SmartStore ajax wrapper
*  Author: Marcus Gesing, SmartStore AG
*/

; (function ($, window, document, undefined) {

	$.fn.doAjax = function (options) {
		normalizeOptions(this, options);

		if (_.isEmpty(options.url)) {
		    console.log('doAjax cannot find the URL!');
		}
		else if (!_.isFalse(options.valid)) {
			doRequestSwitch(options);
		}

		return this.each(function () { });
	};

	$.fn.doAjax.defaults = {
	    /* [...] */
	};

	$.fn.doPostData = function (options) {
		function createAndSubmitForm() {
			var id = 'DynamicForm_' + Math.random().toString().substring(2),
				form = '<form id="' + id + '" action="' + options.url + '" method="' + options.type + '">';

			if (!_.isUndefined(options.data)) {
				$.each(options.data, function (key, val) {
					form += '<input type="hidden" name="' + key + '" value="' + $('<div/>').text(val).html() + '" />';
				});
			}

			form += '</form>';

			$('body').append(form);
			$('#' + id).submit();
		}

		normalizeOptions(this, options);

		if (_.isEmpty(options.url)) {
			console.log('doAjax.doPostData cannot find the URL!');
		}
		else if (_.isEmpty(options.ask)) {
			createAndSubmitForm();
		}
		else if (confirm(options.ask)) {
			createAndSubmitForm();
		}

		return this.each(function () { });
	}


	function normalizeOptions(element, opt) {
		opt.ask = (_.isUndefined(opt.ask) ? $(element).attr('data-ask') : opt.ask);

		if (_.isUndefined(opt.type)) {
			opt.type = 'POST';
		}

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
	        $(opt.throbber).removeData('throbber').throbber({ white: true, small: true, message: '' });
	    }
	    else if (opt.smallIcon) {
	        $(opt.smallIcon).append(window.createCircularSpinner(16, true));
	    }
	}

	function hideAnimation(opt) {
	    if (opt.curtainTitle)
	        $.throbber.hide(true);
	    if (opt.throbber)
	        $(opt.throbber).data('throbber').hide(true);
	    if (opt.smallIcon) {
	        $(opt.smallIcon).find('.spinner').remove();
	    }  
	}

	function doRequest(opt) {
		$.ajax({
			cache: false,
			type: opt.type,
			data: opt.data,
			url: opt.url + (_.isEmpty(opt.appendToUrl) ? '' : opt.appendToUrl),
			async: opt.async === undefined ? true : opt.async,
			beforeSend: function () {
				_.call(opt.callbackBeforeSend);
			},
			success: function (response) {
				_.call(opt.callbackSuccess, response);
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
		if (_.isEmpty(opt.ask)) {
			doRequest(opt);
		}
		else if (confirm(opt.ask)) {
			doRequest(opt);
		}
	}

})(jQuery, window, document);