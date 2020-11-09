/*
*  Project: SmartStore select wrapper 
*  Author: Murat Cakir, SmartStore AG
*/

(function ($, window, document, undefined) {

    // Customize select2 defaults
    $.extend($.fn.select2.defaults.defaults, {
        minimumResultsForSearch: 12,
        theme: 'bootstrap',
        width: 'style', // 'resolve',
        dropdownAutoWidth: false
    });

    var lists = [];

    function load(url, selectedId, callback) {
        $.ajax({
            url: url,
            dataType: 'json',
            async: true,
            data: { selectedId: selectedId || 0 },
            success: function (data, status, jqXHR) {
                lists[url] = data;
                callback(data);
            }
        });
    }

    function initLoad(select, url) {
        $.ajax({
            url: url,
            dataType: 'json',
            success: function (data) {
                _.each(data, function (item) {
                    var option = new Option(item.text, item.id, item.selected, item.selected);
                    select.append(option);
                });
            }
        });
    }

    $.fn.select2.amd.define('select2/data/lazyAdapter', [
        'select2/data/array',
        'select2/utils'
    ],
        function (ArrayData, Utils) {

            function LazyAdapter($element, options) {
                this._isInitialized = false;
                LazyAdapter.__super__.constructor.call(this, $element, options);
            }

            Utils.Extend(LazyAdapter, ArrayData);

            // Replaces the old 'initSelection()' callback method
            LazyAdapter.prototype.current = function (callback) {
                var select = this.$element,
                    opts = this.options.options;

                if (!this._isInitialized) {
                    var init = opts.init || {},
                        initId = init.id || select.data('select-selected-id'),
                        initText = init.text || select.data('select-init-text');

                    if (initId) {
                        // Add the option tag to the select element,
                        // otherwise the current val() will not be resolved.
                        var $option = select.find('option').filter(function (i, elm) {
                            // Do not === otherwise infinite loop ;-)
                            return elm.value == initId;
                        });

                        if ($option.length === 0) {
                            $option = this.option({ id: initId, text: initText, selected: true });
                            this.addOptions($option);
                        }

                        callback([{
                            id: initId,
                            text: initText || ''
                        }]);

                        return;
                    }
                }

                LazyAdapter.__super__.current.call(this, callback);
            };

            LazyAdapter.prototype.query = function (params, callback) {
                var select = this.$element,
                    opts = this.options.options;

                if (!opts.lazy && !opts.lazy.url) {
                    callback({ results: [] });
                }
                else {
                    var url = opts.lazy.url,
                        init = opts.init || {},
                        initId = init.id || select.data('select-selected-id'),
                        term = params.term,
                        list = null;

                    list = lists[url];

                    var doQuery = function (data) {
                        list = data;
                        if (term) {
                            var isGrouped = data.length && data[0].children;
                            if (isGrouped) {
                                // In a grouped list, find the optgroup marked with "main"
                                var mainGroup = _.find(data, function (x) { return x.children && x.main; });
                                data = mainGroup ? mainGroup.children : data[0].children;
                            }
                            list = _.filter(data, function (val) {
                                var rg = new RegExp(term, "i");
                                return rg.test(val.text);
                            });
                        }
                        select.data("loaded", true);
                        callback({ results: list });
                    };

                    if (!list) {
                        load(url, initId, doQuery);
                    }
                    else {
                        doQuery(list);
                    }
                }

                this._isInitialized = true;
            };

            return LazyAdapter;
        }
    );

    $.fn.select2.amd.define('select2/data/remoteAdapter', ['select2/data/array', 'select2/utils'],
        function (ArrayData, Utils) {

            function RemoteAdapter($element, options) {
                this._url = options.get('remoteUrl');
                this._global = options.get('remoteGlobal') || false;
                this._cachedPage = null;
                this._load = true;

                RemoteAdapter.__super__.constructor.call(this, $element, options);
            }

            Utils.Extend(RemoteAdapter, ArrayData);

            RemoteAdapter.prototype.query = function (params, callback) {
                params.page = params.page || 0;
                params.term = params.term || '';

                if (this._load || params.term.length > 0) {

                    // Avoid loading first page multiple times.
                    if (params.page === 0 && params.term.length === 0 && this._cachedPage) {
                        callback({
                            results: this._cachedPage,
                            pagination: { more: true }
                        });
                        return;
                    }

                    var self = this;

                    $.ajax({
                        type: 'GET',
                        url: this._url,
                        global: this._global,
                        dataType: 'json',
                        cache: false,
                        timeout: 5000,
                        data: { page: params.page, term: params.term },
                        success: function (data, status, jqXHR) {
                            self._load = data.hasMoreData || params.term.length > 0;
                            self._cachedPage = null;

                            if (data.hasMoreData && params.page === 0 && params.term.length === 0) {
                                self._cachedPage = data.results;
                            }

                            callback({
                                results: data.results,
                                pagination: { more: data.hasMoreData }
                            });
                        }
                    });
                }
                else {
                    // No more data. Remove "Search..." list item.
                    this.container.$results.find('.loading-results').remove();
                }
            };

            return RemoteAdapter;
        }
    );

    $.fn.selectWrapper = function (options) {
        if (options && !_.str.isBlank(options.resetDataUrl) && lists[options.resetDataUrl]) {
            lists[options.resetDataUrl] = null;
            return this.each(function () { });
        }

        options = options || {};

        var originalMatcher = $.fn.select2.defaults.defaults.matcher;

        return this.each(function () {
            var sel = $(this);

            if (sel.data("select2")) {
                // skip process if select is skinned already
                return;
            }

            if (Modernizr.touchevents && !sel.hasClass("skin") && !sel.data("select-url")) {
                if (sel.find('option[data-color], option[data-imageurl]').length === 0) {
                    // skip skinning if device is mobile and no rich content exists (color & image)
                    return;
                }
            }

            var placeholder = getPlaceholder();

            // following code only applicable to select boxes (not input:hidden)
            var firstOption = sel.children("option").first();
            var hasOptionLabel = firstOption.length &&
                (firstOption[0].attributes['value'] === undefined || _.str.isBlank(firstOption.val()));

            if (placeholder && hasOptionLabel) {
                // clear first option text in nullable dropdowns.
                // "allowClear" doesn't work otherwise.
                firstOption.text("");
            }

            if (placeholder && !hasOptionLabel && !sel.is('[multiple=multiple]')) {
                // create empty first option
                // "allowClear" doesn't work otherwise.
                firstOption = $('<option></option>').prependTo(sel);
            }

            if (!placeholder && hasOptionLabel && firstOption.text() && !sel.data("tags")) {
                // use first option text as placeholder
                placeholder = firstOption.text();
                firstOption.text("");
            }

            function attr(name, value) {
                if (value && value.length > 0) {
                    return ' ' + name + '="' + $('<div/>').text(value).html() + '"';
                }
                return '';
            }

            function renderSelectItem(item, isResult) {
                try {
                    var option = $(item.element),
                        imageUrl = option.data('imageurl'),
                        color = option.data('color'),
                        text = item.text,
                        title = '',
                        preHtml = '',
                        postHtml = '',
                        classes = option.data('item-class') || '',
                        hint = item.hint || option.attr('data-hint'),
                        description = item.description || option.attr('data-description'),
                        icon = option.data('icon'),
                        truncateText = options.maxTextLength > 0 && text.length > options.maxTextLength,
                        appendHint = !isResult && hint && hint.length > 0;

                    if (classes.length > 0) {
                        classes = ' ' + classes;
                    }

                    if (truncateText || appendHint) {
                        title = text;
                        if (appendHint) {
                            title += ' [' + hint + ']';
                        }
                    }

                    if (truncateText) {
                        text = text.substring(0, options.maxTextLength) + '…';
                    }

                    if (isResult) {
                        if (!_.isEmpty(item.id) && !_.isEmpty(item.url)) {
                            if (item.id === '-1') {
                                // Item is a link to open add-entity page.
                                classes += ' select2-item-link prevent-selection';
                            }
                            else {
                                // Add small item button to open detail page.
                                preHtml += '<span class="select2-item-btn float-right">';
                                preHtml += '<a href="' + item.url.replace('__id__', item.id) + '" class="btn btn-flat btn-icon btn-light prevent-selection"' + attr('title', item.urlTitle) + '>';
                                preHtml += '<i class="fa fa-link fa-fw prevent-selection" /></a>';
                                preHtml += '</span>';
                            }
                        }

                        if (!_.isEmpty(description)) {
                            postHtml += '<span class="select2-item-description muted">' + description + '</span>'
                        }
                    }

                    if (imageUrl) {
                        return $(preHtml + '<span class="choice-item' + classes + '"' + attr('title', title) + '><img class="choice-item-img" src="' + imageUrl + '" />' + text + '</span>' + postHtml);
                    }
                    else if (color) {
                        return $(preHtml + '<span class="choice-item' + classes + '"' + attr('title', title) + '><span class="choice-item-color" style="background-color: ' + color + '"></span>' + text + '</span>' + postHtml);
                    }
                    else if (hint && isResult) {
                        return $(preHtml + '<span class="select2-option' + classes + '"><span' + attr('title', title) + '>' + text + '</span><span class="option-hint muted float-right">' + hint + '</span></span>' + postHtml);
                    }
                    else if (icon) {
                        var html = ['<span class="choice-item' + classes + '"' + attr('title', title) + '>'];
                        var icons = _.isArray(icon) ? icon : [icon];
                        var len = (isResult ? 2 : 0) || icons.length;

                        for (i = 0; i < len; i++) {
                            var iconClass = (i < icons.length ? icons[i] + " " : "far ") + "fa-fw mr-2 fs-h6";
                            html.push('<i class="' + iconClass + '" style="width: unset;" />');
                        }

                        html.push(text);
                        html.push('</span>');

                        return html;
                    }
                    else {
                        return $(preHtml + '<span class="select2-option' + classes + '"' + attr('title', title) + '>' + text + '</span>' + postHtml);
                    }
                }
                catch (e) {
                    console.log(e);
                }

                return item.text;
            }

            var opts = {
                allowClear: !!placeholder, // assuming that a placeholder indicates nullability
                placeholder: placeholder,
                templateResult: function (item) {
                    return renderSelectItem(item, true);
                },
                templateSelection: function (item) {
                    return renderSelectItem(item, false);
                },
                closeOnSelect: !sel.prop('multiple'), //|| sel.data("tags"),
                adaptContainerCssClass: function (c) {
                    if (_.str.startsWith(c, "select-"))
                        return c;
                    else
                        return null;
                },
                adaptDropdownCssClass: function (c) {
                    if (_.str.startsWith("drop-"))
                        return c;
                    else
                        return null;
                },
                matcher: function (params, data) {
                    var fallback = true;
                    var terms = $(data.element).data("terms");

                    if (terms) {
                        terms = _.isArray(terms) ? terms : [terms];
                        if (terms.length > 0) {
                            //fallback = false;
                            for (var i = 0; i < terms.length; i++) {
                                if (terms[i].indexOf(params.term) > -1) {
                                    return data;
                                }
                            }
                        }
                    }

                    if (fallback) {
                        return originalMatcher(params, data);
                    }

                    return null;
                }
            };

            if (!options.lazy && sel.data("select-url")) {
                opts.lazy = {
                    url: sel.data("select-url"),
                    loaded: sel.data("select-loaded")
                };
            }

            if (!options.init && sel.data("select-init-text") && sel.data("select-selected-id")) {
                opts.init = {
                    id: sel.data("select-selected-id"),
                    text: sel.data("select-init-text")
                };
            }

            if ($.isPlainObject(options)) {
                opts = $.extend({}, opts, options);
            }

            if (opts.lazy && opts.lazy.url) {
                if (opts.initLoad || sel.data('select-init-load')) {
                    initLoad(sel, opts.lazy.url);
                }

                // url specified: load data remotely (lazily on first open)...               
                opts.dataAdapter = $.fn.select2.amd.require('select2/data/lazyAdapter');
            }
            else if (sel.data('remote-url')) {
                opts.ajax = {};

                if (opts.initLoad || sel.data('select-init-load')) {
                    initLoad(sel, sel.data('remote-url'));
                }

                opts.dataAdapter = $.fn.select2.amd.require('select2/data/remoteAdapter');
            }
            else if (opts.ajax && opts.init && opts.init.text && sel.find('option[value="' + opts.init.text + '"]').length === 0) {
                // In AJAX mode: add initial option when missing
                sel.append('<option value="' + opts.init.id + '" selected>' + opts.init.text + '</option>');
            }

            sel.select2(opts);

            if (sel.hasClass("autowidth")) {
                // move special "autowidth" class to plugin container,
                // so we are able to omit min-width per css
                sel.data("select2").$container.addClass("autowidth");
            }

            function getPlaceholder() {
                return options.placeholder ||
                    sel.attr("placeholder") ||
                    sel.data("placeholder") ||
                    sel.data("select-placeholder");
            }

        });

    };

})(jQuery, window, document);
