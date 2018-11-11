/*
*  Project: SmartStore entity picker
*  Author: Marcus Gesing, SmartStore AG
*/

; (function ($, window, document, undefined) {

	var methods = {
		loadDialog: function (options) {
			options = normalizeOptions(options, this);

			return this.each(function () {
				loadDialog(options);
			});
		},

		initDialog: function () {
			return this.each(function () {
				initDialog(this);
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

	function normalizeOptions(options, context) {
		var self = $(context),
			selector = self.selector;

		var defaults = {
			url: '',
			entity: 'product',
			caption: '&nbsp;',
			disableIf: '',
			disableIds: '',
			thumbZoomer: false,
			highligtSearchTerm: true,
			returnField: 'id',
			returnValueDelimiter: ',',
			returnSelector: '',
			maxReturnValues: 0,
			onLoadDialogBefore: null,
			onLoadDialogComplete: null,
			onOkClicked: null
		};

		options = $.extend({}, defaults, options);

		if (_.isEmpty(options.url)) {
			options.url = self.attr('data-url');
		}

		if (_.isEmpty(options.url)) {
			console.log('entityPicker cannot find the url for entity picker!');
		}

		if (_.isString(selector) && !_.isEmpty(selector) && $(selector).is('input')) {
			options.returnSelector = selector;
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

	function showStatus(dialog, noteClass, condition) {
		var footerNote = $(dialog).find('.footer-note');
		footerNote.find('span').hide();
		footerNote.find('.' + (noteClass || 'default')).show();
	}

	function loadDialog(opt) {
		var dialog = $('#entity-picker-' + opt.entity + '-dialog');

		function showAndFocusDialog() {
			dialog = $('#entity-picker-' + opt.entity + '-dialog');
			dialog.find('.caption').html(opt.caption || '&nbsp;');
			dialog.data('entitypicker', opt);
			dialog.modal('show');

			fillList(dialog, { append: false });

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
				data: {
					"Entity": opt.entity,
					"HighligtSearchTerm": opt.highligtSearchTerm,
					"ReturnField": opt.returnField,
					"MaxReturnValues": opt.maxReturnValues,
					"DisableIf": opt.disableIf,
					"DisableIds": opt.disableIds
				},
				url: opt.url,
				beforeSend: function () {
					if (_.isFunction(opt.onLoadDialogBefore)) {
						return opt.onLoadDialogBefore();
					}
				},
				success: function (response) {
					$('body').append(response);
					showAndFocusDialog();
				},
				complete: function () {
					if (_.isFunction(opt.onLoadDialogComplete)) {
						opt.onLoadDialogComplete();
					}
				},
				error: ajaxErrorHandler
			});
		}
	}

	function initDialog(context) {
		var dialog = $(context),
			keyUpTimer = null,
			currentValue = '';

		// search entities
		dialog.find('button[name=SearchEntities]').click(function (e) {
			e.preventDefault();
			fillList(this, { append: false });
			return false;
		});

		// toggle filters
		dialog.find('button[name=FilterEntities]').click(function () {
			dialog.find('.entity-picker-filter').slideToggle();
		});

		// hit enter or key up starts searching
		dialog.find('input.entity-picker-searchterm').keydown(function (e) {
			if (e.keyCode == 13) {
				e.preventDefault();
				return false;
			}
		}).bind('keyup change paste', function (e) {
			try {
				var val = $(this).val();

				if (val !== currentValue) {
					if (keyUpTimer) {
						keyUpTimer = clearTimeout(keyUpTimer);
					}

					keyUpTimer = setTimeout(function () {
						fillList(dialog, {
							append: false,
							onSuccess: function () {
								currentValue = val;
							}
						});
					}, 500);
				}
			}
			catch (err) { }
		});

		// filter change starts searching
		dialog.find('.entity-picker-filter .item').change(function () {
			fillList(this, { append: false });
		});

	    // lazy loading
		dialog.find('.modal-body').on('scroll', function (e) {
		    if ($('.load-more:not(.loading)').visible(true, false, 'vertical')) {
		        fillList(this, { append: true });
		    }    
		});

		// item select and item hover
		dialog.find('.entity-picker-list').on('click', '.item', function (e) {
			var item = $(this);

			if (item.hasClass('disable'))
				return false;

			var dialog = item.closest('.entity-picker'),
				list = item.closest('.entity-picker-list'),
				data = dialog.data('entitypicker');

			if (data.maxReturnValues === 1) {
				list.find('.item').removeClass('selected');
				item.addClass('selected');
			}
			else if (item.hasClass('selected')) {
				item.removeClass('selected');
			}
			else if (data.maxReturnValues === 0 || list.find('.selected').length < data.maxReturnValues) {
				item.addClass('selected');
			}

			dialog.find('.modal-footer .btn-primary').prop('disabled', list.find('.selected').length <= 0);
		}).on({
			mouseenter: function () {
				if ($(this).hasClass('disable'))
					showStatus($(this).closest('.entity-picker'), 'not-selectable');
			},
			mouseleave: function () {
				if ($(this).hasClass('disable'))
					showStatus($(this).closest('.entity-picker'));
			}
		}, '.item');

		// return value(s)
		dialog.find('.modal-footer .btn-primary').click(function () {
			var dialog = $(this).closest('.entity-picker'),
				items = dialog.find('.entity-picker-list .selected'),
				data = dialog.data('entitypicker'),
				result = '';

			items.each(function (index, elem) {
				var val = $(elem).attr('data-returnvalue');
				if (!_.isEmpty(val)) {
					result = (_.isEmpty(result) ? val : (result + data.returnValueDelimiter + val));
				}
			});

			if (!_.isEmpty(data.returnSelector)) {
				$(data.returnSelector).val(result).focus().blur();
			}

			if (_.isFunction(data.onOkClicked)) {
				if (data.onOkClicked(result)) {
					dialog.modal('hide');
				}
			}
			else {
				dialog.modal('hide');
			}
		});

		// cancel
		dialog.find('button[class=btn][data-dismiss=modal]').click(function () {
			dialog.find('.entity-picker-list').empty();
			dialog.find('.footer-note span').hide();
			dialog.find('.modal-footer .btn-primary').prop('disabled', true);
		});
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
				if (_.isTrue(opt.append)) {
				    dialog.find('.load-more').addClass('loading');
				}
				else {
					dialog.find('.entity-picker-list').empty();
					dialog.find('.modal-footer .btn-primary').prop('disabled', true);
				}

				dialog.find('button[name=SearchEntities]').button('loading').prop('disabled', true);
				dialog.find('.load-more').append(createCircularSpinner(20, true));
			},
			success: function (response) {
				var list = dialog.find('.entity-picker-list'),
					data = dialog.data('entitypicker');

				list.stop().append(response);

				if (_.isFalse(opt.append)) {
					dialog.find('.entity-picker-filter').slideUp();
					showStatus(dialog);
				}

				if (list.thumbZoomer && _.isTrue(data.thumbZoomer)) {
					list.find('.thumb img:not(.zoomable-thumb)').addClass('zoomable-thumb');
					list.thumbZoomer();
				}

				if (_.isFunction(opt.onSuccess)) {
					opt.onSuccess();
				}
			},
			complete: function () {
				dialog.find('button[name=SearchEntities]').prop('disabled', false).button('reset');
				dialog.find('.load-more.loading').parent().remove();
			},
			error: ajaxErrorHandler
		});
	}

})(jQuery, window, document);