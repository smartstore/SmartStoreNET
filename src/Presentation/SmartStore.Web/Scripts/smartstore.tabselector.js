(function ($, window, document, undefined) {

    var tabSelector = window.TabSelector = {

        selectTab: function (strip) {
            var strip = $(strip);
            var current = tabSelector.getSelectedTab(strip);
            var selectedTab = $("#selectedTab").val();

            if (current && !selectedTab)
                return;

            var tab;
            if (!selectedTab) {
                tab = strip.find("a:first");
            }
            else {
                selectedTab = selectedTab.replace(/#/, "");
                tab = strip.find("a[href=#" + selectedTab + "]");
                if (tab.length == 0) {
                    tab = !!(current) ? $(current) : strip.find("a:first");
                }
            }

            if (current) {
                var $current = $(current);
                $current.parent().removeClass("active");
                strip.find(".tab-content " + $current.attr("href")).removeClass("active in");
            }
            
            var hash = tab.attr("href").replace(/#/, "");
            var content = strip.find(".tab-content #" + hash);

            tab.parent().addClass("active"); // LI

            if (content.hasClass("fade")) {
                content.addClass("in");
            }
            content.addClass("active");
        },

        getSelectedTab: function(strip) {
            var strip = $(strip);
            var selected = strip.find('.nav-tabs > li.active > a');

            if (selected.length) {
                return selected[0];
            }

            return null;
        },

        shownHandler: function (e) {
            var hash = $(e.target).attr("href");
            if (hash) {
                hash = hash.replace(/#/, "");
            }
            $("#selectedTab").val(hash);
        }

    };


})( jQuery, this, document );

