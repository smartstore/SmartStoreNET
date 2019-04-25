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

            if (placeholder && !hasOptionLabel) {
                // create empty first option
                // "allowClear" doesn't work otherwise.
                firstOption = $('<option></option>').prependTo(sel);
            }

            if (!placeholder && hasOptionLabel && firstOption.text() && !sel.data("tags")) {
                // use first option text as placeholder
                placeholder = firstOption.text();
                firstOption.text("");
            }

            function renderSelectItem(item, isResult) {
                try {
                    var option = $(item.element),
                        imageUrl = option.data('imageurl'),
                        color = option.data('color'),
                        hint = option.data('hint'),
                        icon = option.data('icon');
                    
                    if (imageUrl) {
                        return $('<span class="choice-item"><img class="choice-item-img" src="' + imageUrl + '" />' + item.text + '</span>');
                    }
                    else if (color) {
                        return $('<span class="choice-item"><span class="choice-item-color" style="background-color: ' + color + '"></span>' + item.text + '</span>');
                    }
                    else if (hint && isResult) {
                        return $('<span class="select2-option"><span>' + item.text + '</span><span class="option-hint muted float-right">' + hint + '</span></span>');
                    }
                    else if (icon) {
                        var html = ['<span class="choice-item">'];
                        var icons = _.isArray(icon) ? icon : [icon];
                        var len = (isResult ? 2 : 0) || icons.length;

                        for (i = 0; i < len; i++) {
                            var iconClass = (i < icons.length ? icons[i] + " " : "far ") + "fa-fw mr-2 fs-h6";
                            html.push('<i class="' + iconClass + '" />');
                        }

                        html.push(item.text);
                        html.push('</span>');

                        return html;
                    }
                    else {
                        return $('<span class="select2-option">' + item.text + '</span>');
                    }
                }
                catch (e) { }

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
                // url specified: load data remotely (lazily on first open)...
                opts.dataAdapter = $.fn.select2.amd.require('select2/data/lazyAdapter');
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
