;
(function ($) {
    $(function () {
    	$(".sm-colorbox").colorpicker({ fallbackColor: false, color: false });
    	$(".sm-colorbox .form-control").on("keyup change input paste", function (e) {
    		var el = $(this);
    		if (!el.val() && el.attr('placeholder')) {
        		el.parent().find('i').css('background-color', el.attr('placeholder'));
        	}
        })
    });
}(jQuery));
