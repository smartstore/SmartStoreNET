/*
*  Project: SmartStore entity picker
*  Author: Marcus Gesing, SmartStore AG
*/

; (function ($, window, document, undefined) {

	var methods = {
		loadDialog: function (options) {
			options = normalizeOptions(options);

			return this.each(function () {
				loadDialog(options);
			});
		},

		fillList: function (options) {
			return this.each(function () {
				fillList(this, options);
			});
		},

		itemClick: function () {
			return this.each(function () {
				itemClick(this);
			});
		},

		returnValues: function () {
			return this.each(function () {
				returnValues(this);
			});
		}
	};

	$.fn.entityPicker = function (method) {
		return main.apply(this, arguments);
	};

	$.entityPicker = function () {
		return main.apply($('.entity-picker:first'), arguments);
	};


	function main(method) {
		if (methods[method])
			return methods[method].apply(this, Array.prototype.slice.call(arguments, 1));

		if (typeof method === 'object' || !method)
			return methods.init.apply(this, arguments);

		EventBroker.publish("message", { title: 'Method "' + method + '" does not exist on jQuery.entityPicker', type: "error" });
		return null;
	}

	function normalizeOptions(options) {
		var defaults = {
			entity: 'product',
			url: '',
			thumbZoomer: false,
			returnValue: 'id',
			returnValueDelimiter: ';',
			maxReturnValues: 1
		};

		options = $.extend({}, defaults, options);

		if (_.isEmpty(options.url)) {
			options.url = $(element).attr('data-url');
		}

		if (_.isEmpty(options.url)) {
			console.log('entityPicker cannot find the url for entity picker!');
		}

		return options;
	}

	function ajaxErrorHandler(objXml) {
		try {
			if (objXml != null && objXml.responseText != null && objXml.responseText !== '') {
				EventBroker.publish("message", { title: objXml.responseText, type: "error" });
			}
		}
		catch (e) { }
	}

	function loadDialog(opt) {
		var dialog = $('#entity-picker-' + opt.entity + '-dialog');

		function showAndFocusDialog() {
			dialog.modal('show');
			setTimeout(function () {
				dialog.find('.modal-body :input:visible:enabled:first').focus();
			}, 800);
		}

		if (dialog.length) {
			showAndFocusDialog();
		}
		else {
			$.ajax({
				cache: false,
				type: 'GET',
				data: { "Entity": opt.entity, "MultiPick": opt.multiPick },
				url: opt.url,
				success: function (response) {
					$('body').append(response);
					dialog = $('#entity-picker-' + opt.entity + '-dialog');
					dialog.find('.caption').html(opt.caption || '&nbsp;');
					dialog.data('entitypicker', opt);
					showAndFocusDialog();
				},
				error: ajaxErrorHandler
			});
		}
	}

	function fillList(context, opt) {
		var dialog = $(context).closest('.entity-picker');

		if (_.isTrue(opt.append)) {
			var pageElement = dialog.find('input[name=PageIndex]'),
				pageIndex = parseInt(pageElement.val());

			pageElement.val(pageIndex + 1);
		}
		else {
			dialog.find('input[name=PageIndex]').val('0');
		}

		$.ajax({
			cache: false,
			type: 'POST',
			data: dialog.find('form:first').serialize(),
			url: dialog.find('form:first').attr('action'),
			beforeSend: function () {
				if (_.isTrue(opt.append))
					dialog.find('.list-footer').remove();
				else
					dialog.find('.entity-picker-list:first').empty();

				dialog.find('button[name=SearchEntities]').button('loading').prop('disabled', true);
				dialog.find('.entity-picker-list:first').append('&nbsp;<span class="ajax-loader-small"></span>');
			},
			success: function (response) {
				var list = dialog.find('.entity-picker-list:first'),
					data = dialog.data('entitypicker');

				list.append(response);

				if (!_.isTrue(opt.append)) {
					dialog.find('.footer-note').show();
					dialog.find('.entity-picker-filter').slideUp();
				}

				if (list.thumbZoomer && _.isTrue(data.thumbZoomer)) {
					list.find('.thumb img:not(.zoomable-thumb)').addClass('zoomable-thumb');
					list.thumbZoomer();
				}
			},
			complete: function () {
				dialog.find('button[name=SearchEntities]').prop('disabled', false).button('reset');
				dialog.find('.entity-picker-list:first').find('span.ajax-loader-small').remove();
			},
			error: ajaxErrorHandler
		});
	}

	function itemClick(item) {
		var dialog = $(item).closest('.entity-picker'),
			list = $(item).closest('.entity-picker-list'),
			data = dialog.data('entitypicker');

		if (data.maxReturnValues === 1) {
			list.find('.item').removeClass('selected');
			$(item).addClass('selected');
		}
		else if ($(item).hasClass('selected')) {
			$(item).removeClass('selected');
		}
		else if (list.find('.selected').length < data.maxReturnValues) {
			$(item).addClass('selected');
		}

		dialog.find('.modal-footer .btn-primary').prop('disabled', list.find('.selected').length <= 0);
	}

	function returnValues(context) {
		var dialog = $(context).closest('.entity-picker');

	}

})(jQuery, window, document);