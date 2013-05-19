;
(function ($) {
    $(function () {
        $(".sm-colorbox").colorpicker();
        $(".sm-colorbox .colorval").on("keyup change", function (e) {
            $(this).parent().data("colorpicker").setValue($(this).val());
        })
    });
}(jQuery));
