var Hacks = {
	Telerik: {
		handleButton: function (el) {
			// TODO: temp > Handle Button with MVC
			el.each(function () {
				var btn = $(this);
				if (btn.hasClass("t-grid-add") && btn.find('> .fa').length === 0) {
					btn.prepend('<i class="fa fa-plus mr-2"></i>');
				}
				else if (btn.hasClass("t-grid-save-changes") && btn.find('> .fa').length === 0) {
					btn.prepend('<i class="fa fa-check mr-2"></i>');
				}
			});
		},

		handleGridFilter: function () {
			// Because we restyled the grid, the filter dropdown does not position
			// correctly anymore. We have to reposition it.
			if ($.telerik && $.telerik.filtering) {
				var showFilter = $.telerik.filtering.implementation.showFilter;
                $.telerik.filtering.implementation.showFilter = function (e) {
                    // The filter button
                    var btn = $(e.currentTarget);
                    // Call the original func
                    showFilter.apply(this, [e]);
                    // The filter dropdown
                    var filter = btn.data('filter');
                    var grid = filter.parent();
                    filter.css({
                        left: (btn.offset().left - grid.offset().left) + "px"
                    });

                    // wrap with new relative element to circumvent cropping of filter dialog
                    if (!grid.hasClass("wrapped")) {
                        grid.addClass("wrapped")
                            .css("position", "initial")
                            .wrap("<div class='position-relative'></div>");
                    }
				};
			}
		}
	}
};