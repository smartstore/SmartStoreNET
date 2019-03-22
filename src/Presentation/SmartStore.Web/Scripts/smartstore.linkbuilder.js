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
            self.queryStringIcon = $el.find('.link-type[data-type="query-string"] > i');

            // set type selector to initial type from current expression
            _.delay(function () {
                var currentType = $el.find('a[data-type="' + self.currentType + '"]');
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
        queryStringIcon: null,

        initControl: function () {

            var self = this,
                $el = $(self.element);

            // switch link type
            $el.on("click", ".link-type", function(e) {
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
            $el.on("change", ".transferable", function (e) {
                var $el = $(this),
                    type = $el.closest('.link-control').data('type'),
                    val, qs = self._getQueryString();

                // get link excluding query string
                if (type === 'query-string') {
                    val = self.templateValueField.val();
                    var index = val.indexOf('?');
                    if (index !== -1) {
                        val = val.substring(0, index);
                    }
                }
                else {
                    val = $el.val();
                    if (!_.isEmpty(val) && type !== 'url') {
                        val = type + ':' + val;
                    }
                }

                // append query string
                if (type !== 'url' && !_.isEmpty(val) && !_.isEmpty(qs)) {
                    val = (val || '') + '?' + qs;
                }

                // update queryStringIcon to indicate whether a query string is set
                self.queryStringIcon.attr('class', _.isEmpty(qs) ? 'fas fa-minus text-muted' : 'fas fa-check text-success');

                self.templateValueField.val(val).trigger("change");
                //console.log('change ' + self.templateValueField.val());
            });

            // reset control
            $el.on("click", ".btn-reset", function (e) {
                
                self.templateValueField.val('');
                self.queryStringCtrl.val('');
                self.controls.find('.resettable:visible').val('').trigger('change');
                self.queryStringIcon.attr('class', 'fas fa-minus text-muted');

                var select2 = self.controls.not("hide").find(".select2-hidden-accessible");
                if (select2.length > 0) {
                    select2.select2("val", "");
                }
            });

            // browse files
            $el.on("click", ".browse-files", function (e) {
                e.preventDefault();
                var el = $(this),
                    url = el.data('url');

                url = modifyUrl(url, 'type', '#');
                url = modifyUrl(url, 'field', el.data('field-id'));
                url = modifyUrl(url, 'mid', 'modal-browse-files');

                openPopup({
                    id: 'modal-browse-files',
                    url: url,
                    flex: true,
                    large: true,
                    backdrop: false
                });
            });
        },

        _updateTypeInfo: function (el) {

            if (!el) return;

            var self = this,
                $el = $(self.element);

            var type = el.data('type');
            if (type === 'query-string')
                return;

            var btn = $el.find('.dropdown-toggle'),
                icon = el.find('i').attr('class').replace('fa-fw ', ''),
                name = el.find('span').text();

            btn.find('i').attr('class', icon);
            btn.attr('title', name);

            $el.find('.qs-menu-container').toggle(type !== 'url');
        },

        _getQueryString: function () {
            var self = this;
            var val = self.queryStringCtrl.val() || '';

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

})( jQuery, this, document );