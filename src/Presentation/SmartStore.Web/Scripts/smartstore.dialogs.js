(function ($, window) {

	$(function () {
		// Global modal event handlers
		$(document).on('show.bs.modal', '.modal', function () {
			if ($(this).data('backdrop') || $('body > .modal-backdrop').length) {
				$('body').addClass('modal-has-backdrop');
            }
		});
		$(document).on('hidden.bs.modal', '.modal', function () {
			$('body').removeClass('modal-has-backdrop');
		});
	});

	function createBoxModal(type, id, opts) {
		var dialogClass = 'modal-dialog';
		if (opts.size === 'sm' || opts.size === 'lg') dialogClass += ' modal-' + opts.size;
		if (toBool(opts.center, true)) dialogClass += ' modal-dialog-centered';

		var html = [
			'<div id="modal-confirm-shared" class="modal fade modal-box modal-{0}" data-backdrop="{1}" role="dialog" aria-hidden="true" tabindex="-1">'.format(type, toBool(opts.backdrop, true)),
				'<div class="{0}" role="document">'.format(dialogClass),
					'<div class="modal-content rounded-sm">',
						'<div class="modal-body">',
							'<div class="modal-box-body"><div><i class="far fa-question-circle fa-3x text-success mb-3"></i></div>',
								'<div class="modal-box-message">' + opts.message + '</div>',
							'</div>',
						'</div>',
						'<div class="modal-footer d-flex justify-content-center">',
							'<button type="button" class="btn btn-primary btn-sm btn-accept" data-dismiss="modal">' + window.Res['Common.OK'] + '</button>',
							'<button type="button" class="btn btn-secondary btn-sm btn-cancel" data-dismiss="modal">' + window.Res['Common.Cancel'] + '</button>',
						'</div>',
					'</div>',
				'</div>',
			'</div>'
		].join("");

		var modal = $(html).appendTo('body');

		if (toBool(opts.centerContent, true)) {
			modal.addClass('modal-box-center');
		}

		if (opts.title) {
			var header = [
				'<div class="modal-header">',
					'<h5 class="modal-title">' + opts.title + '</h5>',
					'<button type="button" class="close" data-dismiss="modal" aria-label="Close">&times;</button>',
				'</div>'
			].join("");
			$(header).prependTo(modal.find('.modal-content'));
		}

		if (opts.type) {
			var boxBody = modal.find('.modal-box-body');
			if (opts.type === 'question') {
				// ...
			}
			// ...
		}

		modal.on('shown.bs.modal', function (e) {
			modal.find('.btn-accept').first().trigger('focus');
		});
		modal.on('hidden.bs.modal', function (e) {
			if ((type === 'confirm' || type === 'prompt') && _.isFunction(opts.callback)) {
				opts.callback.apply(this, [modal.data('accept')]);
			}
			modal.remove();
		});

		modal.on('click', '.btn-accept', function () {
			modal.data('accept', true);
		});

		return modal;
    }

	window.confirm2 = function (message, callback) {
		var opts = $.isPlainObject(message) ? message : {
			backdrop: true,
			title: null,
			icon: null,
			center: true,
			centerContent: true,
			size: 'md', // sm | md | lg
			message: message,
			type: null, // question | warning | danger | info
			callback: callback
		};

		var modal = $('#modal-confirm-shared');
		if (modal.length)
			return;

		return createBoxModal('confirm', 'modal-confirm-shared', opts).modal('show');
    }

	window.popup = window.openPopup = function (url, large, flex) {
		var opts = $.isPlainObject(url) ? url : {
			/* id, backdrop */
			url: url,
			large: large,
			flex: flex
		};

		var id = (opts.id || "modal-popup-shared");
		var modal = $('#' + id);
		var iframe;
		var sizeClass = "";

		if (opts.flex === undefined) opts.flex = true;
		if (opts.flex) sizeClass = "modal-flex";
		if (opts.backdrop === undefined) opts.backdrop = true;

		if (opts.large && !opts.flex)
			sizeClass = "modal-lg";
		else if (!opts.large && opts.flex)
			sizeClass += " modal-flex-sm";

		if (modal.length === 0) {
			var html = [
				'<div id="' + id + '" class="modal fade" data-backdrop="' + opts.backdrop + '" role="dialog" aria-hidden="true" tabindex="-1" style="border-radius: 0">',
					'<a href="javascript:void(0)" class="modal-closer d-none d-md-block" data-dismiss="modal" title="' + window.Res['Common.Close'] + '">&times;</a>',
					'<div class="modal-dialog{0} modal-dialog-app" role="document">'.format(sizeClass.length ? ' ' + sizeClass : ''),
						'<div class="modal-content">',
							'<div class="modal-body">',
								'<iframe class="modal-flex-fill-area" frameborder="0" src="' + opts.url + '" />',
							'</div>',
							'<div class="modal-footer d-md-none">',
								'<button type="button" class="btn btn-secondary btn-sm btn-default" data-dismiss="modal">' + window.Res['Common.Close'] + '</button>',
							'</div>',
						'</div>',
					'</div>',
				'</div>'
			].join("");

            modal = $(html).appendTo('body').on('hidden.bs.modal', function (e) {
                // Cleanup
                $(modal.find('iframe').attr('src', 'about:blank')).remove();
				modal.remove();
			});

			// Create spinner
			var spinner = $('<div class="spinner-container w-100 h-100 active" style="position:absolute; top:0; background:#fff; border-radius:4px"></div>').append(createCircularSpinner(64, true, 2));
			modal.find('.modal-body').append(spinner);

			iframe = modal.find('.modal-body > iframe');
			iframe.on('load', function (e) {
				modal.find('.modal-body > .spinner-container').removeClass('active');
			});
		}
		else {
			iframe = modal.find('.modal-body > iframe');
			modal.find('.modal-body > .spinner-container').addClass('active');
			iframe.attr('src', opts.url);
		}

		if (_.isFunction(opts.onMessage)) {
			$(iframe.get(0).contentWindow).one('message', function (e) {
				var result = e.originalEvent.data;
				opts.onMessage.apply(this, [result]);
			});
        }

		modal.modal('show');

		return iframe.get(0);
	}

	window.closePopup = function (id) {
		var modal = $('#' + (id || "modal-popup-shared"));
		if (modal.length > 0) {
			modal.modal('hide');
		}
	}

})( jQuery, this );

