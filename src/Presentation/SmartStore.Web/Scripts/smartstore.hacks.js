
var Hacks = {

    Telerik: {
        // TODO: temp > Handle Button with MVC
        handleButton: function (el) {
            el.removeClass("t-button").addClass("btn");
            el.each(function () {
                var btn = $(this);
                if (btn.hasClass("t-grid-add")) {
                    btn.addClass("btn-link").prepend('<i class="fa fa-plus"></i>&nbsp;');
                }
                else if (btn.hasClass("t-grid-save-changes")) {
                	btn.addClass("btn-primary").prepend('<i class="fa fa-check"></i>&nbsp;');
                }
                else if (btn.hasClass("t-grid-cancel-changes")) {
                	btn.addClass("btn-link");
                }
            });

        }
    }

}