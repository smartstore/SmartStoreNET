/*
*  Project: SmartStore select wrapper 
*  Author: Murat Cakir, SmartStore AG
*/
;
(function ($, window, document, undefined) {

	var lists = [];

	function load(url, selectedId) {
		$.ajax({
			url: url,
			dataType: 'json',
			async: false,
			data: { selectedId: selectedId || 0 },
			success: function (data, status, jqXHR) {
				lists[url] = data;
			}
		});
	};

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
					opts = this.options.options,
					data = [];

				if (!this._isInitialized) {
					callback([{
						id: opts.init ? opts.init.id : null,
						text: opts.init ? opts.init.text : null
					}]);
				}
				else {
					LazyAdapter.__super__.current.call(this, callback);
				}
			};

			LazyAdapter.prototype.query = function (params, callback) {
				var opts = this.options.options;

				if (!opts.lazy && !opts.lazy.url) {
					callback({ results: [] });
				}

				var url = opts.lazy.url;
				var selectedId = opts.init ? opts.init.id : null;
				var term = params.term;

				if (!lists[url]) {
					load(url, selectedId);
				}

				var list;
				if (!term) {
					list = lists[url];
				}
				else {
					list = _.filter(lists[url], function (val) {
						return new RegExp(term, "i").test(val.text);
					});
				}

				var data = { results: list };
				callback(data);

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

        return this.each(function () {
			var sel = $(this);

            if (sel.data("select2")) { 
                // skip process if select is skinned already
                return;
            }
            
            if (Modernizr.touchevents && !sel.hasClass("skin")) {
            	if (sel.find('option[data-color], option[data-imageurl]').length == 0) {
					// skip skinning if device is mobile and no rich content exists (color & image)
            		return;
            	}
            }

			if (!options.lazy && sel.data("select-url")) {
				options.lazy = {
					url: sel.data("select-url"),
					loaded: sel.data("select-loaded")
				}
			}

			if (!options.init && sel.data("select-init-text") && sel.data("select-selected-id")) {
				options.init = {
					id: sel.data("select-selected-id"),
					text: sel.data("select-init-text")
				}
			}

            var autoWidth = sel.hasClass("autowidth"),
                minResultsForSearch = sel.data("select-min-results-for-search"),
                minInputLength = sel.data("select-min-input-length"),
				noCache = sel.data("select-nocache"), // future use
				lazy = options.lazy,
				url = options.lazy ? options.lazy.url : null,
				loaded = options.lazy ? options.lazy.loaded : null,
				initText = options.init ? options.init.text : null,
				selectedId = options.init ? options.init.id : null;

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

            function renderSelectItem(item) {
            	try {
            		var option = $(item.element),
						imageUrl = option.data('imageurl'),
            			color = option.data('color');

					if (imageUrl) {
            			return $('<span><img class="choice-item-img" src="' + imageUrl + '" />' + item.text + '</span>');
            		}
            		else if (color) {
            			return $('<span><span class="choice-item-color" style="background-color: ' + color + '"></span>' + item.text + '</span>');
					}
					else {
						return $('<span class="select2-option">' + item.text + '</span>');
					}
            	}
            	catch (e) { }

            	return item.text;
            }

            var opts = {
            	width: 'style', // 'resolve',
            	dropdownAutoWidth: false,
                allowClear: !!(placeholder), // assuming that a placeholder indicates nullability
                placeholder: placeholder,
                minimumResultsForSearch: _.isNumber(minResultsForSearch) ? minResultsForSearch : 8,
                minimumInputLength: _.isNumber(minInputLength) ? minInputLength : 0,
                templateResult: renderSelectItem,
                templateSelection: renderSelectItem,
				theme: 'bootstrap',
				closeOnSelect: !(sel.prop('multiple') || sel.data("tags")),
				adaptContainerCssClass: function (c) {
					if (c.startsWith("select-"))
						return c;
					else
						return null;
				},
				adaptDropdownCssClass: function (c) {
					if (c.startsWith("drop-"))
						return c;
					else
						return null;
				}
			};

			if ($.isPlainObject(options)) {
				opts = $.extend({}, opts, options);
			}

            if (url) {
                // url specified: load data remotely...
                if (lazy) {
                    // ...but lazy (on first open)
					opts.dataAdapter = $.fn.select2.amd.require('select2/data/lazyAdapter');
                }
                else {
                    // ...immediately
                    buildOptions();
                }
			}
			else if (opts.ajax && opts.init && opts.init.text && sel.find('option[value="' + opts.init.text + '"]').length === 0) {
				// In AJAX mode: add initial option when missing
				sel.append('<option value="' + opts.init.id + '" selected>' + opts.init.text + '</option>');
			}

			sel.select2(opts);

            if (autoWidth) {
                // move special "autowidth" class to plugin container,
            	// so we are able to omit min-width per css
                sel.data("select2").$container.addClass("autowidth");
            }

            function buildOptions() {
                if (!lists[url]) {
					load(url, selectedId);
                }

                // create option tags
                if (!loaded) {
                    $.each(lists[url], function () {
                        var o = $(document.createElement('option'))
                                    .attr('value', this.id)
                                    .text(this.text || this.name)
                                    .appendTo(sel);
                        if (this.selected) {
                            o.attr("selected", "selected");
                        }
                    })

                    // mark select as 'filled'
                    sel.data("loaded", true);
                }
            }

			function getPlaceholder() {
				return (options && options.placeholder)
					sel.attr("placeholder") ||
					sel.data("placeholder") ||
					sel.data("select-placeholder");
            }

        });

    }

})(jQuery, window, document);
