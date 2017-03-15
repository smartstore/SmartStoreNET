;
(function ($) {
    $(function () {
        $(".sm-colorbox").colorpicker();
        $(".sm-colorbox .colorval").on("keyup change", function (e) {
        	var el = $(this);
        	var picker = el.parent().data("colorpicker");
        	var val = el.val();
        	picker.setValue(val || el.attr('placeholder'));
        })
    });
}(jQuery));
