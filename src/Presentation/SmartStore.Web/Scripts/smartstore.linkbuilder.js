;
(function ($, window, document, undefined) {

    var pluginName = 'linkBuilder';

    function LinkBuilder(element, options) {

        var self = this;
        this.element = element;
        var $el = this.el = $(element);

        self.init = function () {

            // init
            self.currentType = $el.data("current-type");
            self.controls = $el.find(".link-control");
            self.templateValueField = $el.find("#" + $el.data("field-id"));
            self.queryStringCtrl = $el.find(".query-string");
            self.typeContainer = $el.find(".type-container");

            _.delay(function () {

                // hide dropdown-menu if only one type is available
                self._tryHideTypeContainer();

                // set type selector to initial type from current expression
                var currentType = $el.find('a[data-type="' + self.currentType + '"]');

                if (self.currentType != "")
                    self._updateTypeInfo(currentType);

            }, 100);

            // init controls
            self.controls.find('select').selectWrapper();
            self.controls.find('.select2-container').addClass('w-100');

            self.initControl();
        };

        self.init();
    }

    LinkBuilder.prototype = {
        controls: null,
        currentType: null,
        templateValueField: null,
        queryStringCtrl: null,

        initControl: function () {

            var self = this,
                $el = $(self.element);

            // switch link type
            $el.on("click", ".link-type", function (e) {
                e.preventDefault();

                var el = $(this);
                self.controls.hide();
                self._updateTypeInfo(el);

                var currentCtrl = self.controls
                    .filter('[data-type="' + el.data('type') + '"]')
                    .show();

                // focus input elem to indicate next user interaction
                _.delay(function () { currentCtrl.find(':input:not([readonly])').focus(); }, 100);
            });

            // build expression & transfer to editor template value field
            $el.on("change", ".transferable", function () {
                var ctrl = $(this),
                    type = ctrl.closest('.link-control').data('type') || '',
                    val, qs = self._getQueryString();

                // get link excluding query string
                if (ctrl.hasClass('query-string')) {
                    val = self.templateValueField.val();
                    var index = val.indexOf('?');
                    if (index !== -1) {
                        val = val.substring(0, index);
                    }
                }
                else {
                    val = ctrl.val();
                    if (!_.isEmpty(val) && type !== 'url') {
                        val = type + ':' + val;
                    }
                }

                // append query string
                if (type !== 'url' && !_.isEmpty(val) && !_.isEmpty(qs)) {
                    val = (val || '') + '?' + qs;
                }

                self._updateQueryStringIcon(!_.isEmpty(qs));

                self.templateValueField.val(val).trigger("change");
                //console.log('change ' + self.templateValueField.val());
            });

            // reset control
            $el.on("click", ".btn-reset", function () {
                self.templateValueField.val('');
                self.queryStringCtrl.val('');
                self.controls.find('.resettable:visible').val('').trigger('change');
                self._updateQueryStringIcon(false);

                // Really reset select2 completely.
                var select2 = self.controls.find('.select2:visible');
                if (select2.length) {
                    var label = select2.find('.selection .select2-selection__rendered');
                    if (label.length) {
                        label.removeAttr('title').html('');
                    }
                }
            });

            // browse files
            $el.on("click", ".browse-files", function (e) {
                e.preventDefault();
                var fieldId = $(this).data('field-id');

                SmartStore.media.openFileManager({
                    el: this,
                    backdrop: false,
                    onSelect: function (files) {
                        if (!files.length) return;
                        $('#' + fieldId).val(files[0].url).trigger('change');
                    }
                });
            });
        },

        _updateTypeInfo: function (elem) {
            if (!elem) return;

            var cnt = $(this.element),
                type = elem.data('type');

            var btn = cnt.find('.btn-icon'),
                icon = elem.find('i').attr('class').replace('fa-fw ', ''),
                name = elem.find('span').text();

            btn.find('i').attr('class', icon);
            btn.attr('title', name);

            cnt.find('.btn-query-string').toggle(type !== 'url');
        },

        _tryHideTypeContainer: function (hasQueryString) {
            var self = this;
            var types = self.typeContainer.find(".dropdown-menu .dropdown-item");
            if (types.length === 1) {

                // find the only dropdown-item and trigger click to display the correct type
                self.typeContainer
                    .find(".dropdown-item")
                    .trigger("click");

                // Prevent dropdown-menu from being displayed
                self.typeContainer
                    .find(".btn-icon")
                    .removeClass("dropdown-toggle")
                    .removeAttr("data-toggle");
            }
        },

        _updateQueryStringIcon: function (hasQueryString) {
            $(this.element).find('.btn-query-string > i')
                .removeClass('text-muted text-success')
                .addClass(hasQueryString ? 'text-success' : 'text-muted');
        },

        _getQueryString: function () {
            var val = this.queryStringCtrl.val() || '';

            while (val.startsWith('?')) {
                val = val.substring(1);
            }

            return val;
        }
    };

    // the global, default plugin options
    var defaults = {
        // [...]
    };

    $[pluginName] = { defaults: defaults };

    $.fn[pluginName] = function (options) {
        return this.each(function () {
            if (!$.data(this, pluginName)) {
                options = $.extend({}, $[pluginName].defaults, options);
                $.data(this, pluginName, new LinkBuilder(this, options));
            }
        });
    };

})(jQuery, this, document);