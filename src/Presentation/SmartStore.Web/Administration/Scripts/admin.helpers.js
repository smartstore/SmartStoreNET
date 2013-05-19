/// <reference path="../../Scripts/jquery-1.8.3.js" />
/// <reference path="../../Scripts/underscore.js" />
/// <reference path="../../Scripts/underscore.string.js" />
/// <reference path="../../Scripts/underscore.mixins.js" />
/// <reference path="admin.common.js" />

// NOT IN USE, should delete later

// codehint: sm-add
Admin.Helpers = (function ($) {
    
    var defaults = opts = {
        urls: {
            categories: null,
            manufacturers: null,
            customerRoles: null,
            productAttributes: null
        }
    };

    var lists = [];
    function load(entity) {
        $.ajax({
            url: opts.urls[entity],
            dataType: 'json',
            async: false,
            data: { selectedId: 0 },
            success: function (data, status, jqXHR) {
                lists[entity] = data;
            }
        });
    };

    function prepareDropdown(entity, dd /* the select element(s) */) {
        if (!dd || dd.length == 0) {
            // nothing to be done here
            return;
        }
        if (!lists[entity]) {
            // load entities
            load(entity);
        }

        // create options
        if (!dd.data("loaded")) {
            $.each(lists[entity], function () {
                var o = $(document.createElement('option'))
                            .attr('value', this.id)
                            .text(this.text)
                            .appendTo(dd);
                if (this.selected) {
                    o.attr("selected", "selected");
                }
            })

            // mark select as 'filled'
            dd.data("loaded", true);
        }
    };

    return {

        init: function (options) {
            $.extend(defaults, true, options);
        },

        getList: function (entity) {
            if (!lists[entity]) {
                // load entities
                load(entity);
            }
            return lists[entity];
        },

        prepareCategoriesDropdown: function (dd) {
            prepareDropdown("categories", dd);
        },

        prepareManufacturersDropdown: function (dd) {
            prepareDropdown("manufacturers", dd);
        },

        prepareCustomerRolesDropdown: function (dd) {
            prepareDropdown("customerRoles", dd);
        },

        prepareProductAttributesDropdown: function (dd) {
            prepareDropdown("productAttributes", dd);
        }

    };

})(jQuery);
