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




    var iconHints = {
        check: { name: 'fa fa-check', color: 'success' },
        question: { name: 'far fa-question-circle', color: 'warning' },
        danger: { name: 'fa fa-exclamation-triangle', color: 'danger' },
        info: { name: 'fa fa-info', color: 'info' },
        warning: { name: 'fa fa-exclamation-circle', color: 'warning' },
        delete: { name: 'fa fa-trash-alt', color: 'danger' }
    };

    function createBoxModal(type, opts) {
		/*
		 * ==============================
			opts = {
				backdrop: true,
				title: null,
				center: true,
				centerContent: true,
				size: 'md', // sm | md | lg
				message: message,
				icon: null, // { type: 'check | question | warning | danger | info', name: <FaIconClass>, color: <BrandColor> }
				prompt: null // { value: <string>, onInit: <fn>, invalidChars: <string> }
				callback: callback,
				show: true
			};
		* ==============================
		*/

        var dialogClass = 'modal-dialog modal-dialog-scrollable',
            center = toBool(opts.center, true),
            centerContent = toBool(opts.centerContent, type !== 'prompt');

        if (opts.size === 'sm' || opts.size === 'lg') dialogClass += ' modal-' + opts.size;
        if (center) dialogClass += ' modal-dialog-centered';
        if (centerContent) dialogClass += ' modal-box-center';

        var html = [
            '<div id="modal-{0}-shared" class="modal fade modal-box modal-{0}" data-backdrop="{1}" role="dialog" aria-hidden="true">'.format(type, toBool(opts.backdrop, true)),
            '<div class="{0}" role="document">'.format(dialogClass),
            '<div class="modal-content rounded-sm">',
            '<div class="modal-body">',
            '<div class="modal-box-body d-flex{0}">'.format(centerContent || type === 'prompt' ? '' : ' flex-nowrap'),
            !opts.message ? '' : '<div class="modal-box-message">{0}</div>'.format(opts.message),
            '</div>',
            '</div>',
            '<div class="modal-footer d-flex">',
            '<button type="button" class="btn btn-primary btn-sm btn-accept" data-dismiss="modal" tabindex="2">' + window.Res['Common.OK'] + '</button>',
            '<button type="button" class="btn btn-secondary btn-sm btn-cancel" data-dismiss="modal" tabindex="3">' + window.Res['Common.Cancel'] + '</button>',
            '</div>',
            '</div>',
            '</div>',
            '</div>'
        ].join("");

        var modal = $(html).appendTo('body');

        if (type === 'alert') {
            modal.find('.modal-footer > .btn-cancel').remove();
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

        var boxBody = modal.find('.modal-box-body');
        var color;

        if (opts.icon && (opts.icon.type)) {
            var hint = iconHints[opts.icon.type];
            if (hint) {
                if (opts.icon.name) hint.name = opts.icon.name;
                color = opts.icon.color || hint.color;
                if (type === 'confirm' && color === 'danger') modal.find('.btn-accept').addClass('btn-to-danger');
                var icon = $('<i class="{0} text-{1} fa-fw fa-3x"></i>'.format(hint.name, opts.icon.color || hint.color));
                icon.addClass('m{0}-3'.format(centerContent ? 'b' : 'r'));
                boxBody.prepend(icon.wrap('<div class="modal-box-icon"></div>').parent());
            }
        }

        var input;
        if (type === 'prompt') {
            input = $('<input type="text" class="form-control prompt-control" autocomplete="off" tabindex="1" />');
            if (opts.prompt) {
                if (opts.prompt.value) {
                    input.val(opts.prompt.value);
                }
                if (opts.prompt.invalidChars) {
                    input.on('keydown', function (e) {
                        if (opts.prompt.invalidChars.indexOf(e.key) > -1) {
                            e.preventDefault();
                        }
                    });
                }
            }

            boxBody.append(input.wrap('<div class="modal-box-input w-100"></div>').parent());
        }

        modal.on('shown.bs.modal', function (e) {
            (input || modal.find('.btn-accept')).first().trigger('focus');
            if (input) {
                var fn = opts.prompt ? opts.prompt.onInit : null;
                if (_.isFunction(fn)) {
                    fn.apply(input.get(0), [input.get(0)]);
                }
                else {
                    input.select();
                }
            }
        });
        modal.on('hidden.bs.modal', function (e) {
            if (type !== 'alert' && _.isFunction(opts.callback)) {
                var accepted = modal.data('accept');
                if (input) {
                    if (accepted) opts.callback.apply(this, [input.val()]);
                }
                else {
                    opts.callback.apply(this, [accepted]);
                }
            }
            modal.remove();
        });

        modal.on('click', '.btn-accept', function () {
            modal.data('accept', true);
        });

        return modal;
    }

    window.alert2 = function (message) {
        var opts = $.isPlainObject(message) ? message : { message: message };

        var modal = $('#modal-alert-shared');
        if (modal.length)
            modal.remove();

        modal = createBoxModal('alert', opts);

        if (toBool(opts.show, true))
            modal.modal('show');

        return modal;
    }

    window.confirm2 = function (message, callback) {
        var opts = $.isPlainObject(message) ? message : { message: message, callback: callback };

        var modal = $('#modal-confirm-shared');
        if (modal.length)
            modal.remove();

        modal = createBoxModal('confirm', opts);

        if (toBool(opts.show, true))
            modal.modal('show');

        return modal;
    }

    window.prompt2 = function (message, callback) {
        var opts = $.isPlainObject(message) ? message : { message: message, callback: callback };

        var modal = $('#modal-prompt-shared');
        if (modal.length)
            modal.remove();

        modal = createBoxModal('prompt', opts);

        if (toBool(opts.show, true))
            modal.modal('show');

        return modal;
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

})(jQuery, this);

