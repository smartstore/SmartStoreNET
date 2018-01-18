
var Hacks = {

    Telerik: {
        // TODO: temp > Handle Button with MVC
		handleButton: function (el) {
            el.removeClass("t-button").addClass("btn");
            el.each(function () {
				var btn = $(this);

                if (btn.hasClass("t-grid-add")) {
                    btn.addClass("btn-warning").prepend('<i class="fa fa-plus mr-2"></i>');
                }
                else if (btn.hasClass("t-grid-save-changes")) {
                	btn.addClass("btn-primary").prepend('<i class="fa fa-check mr-2"></i>');
                }
                else if (btn.hasClass("t-grid-cancel-changes")) {
                	btn.addClass("btn-warning");
                }
            });

        }
    }

}