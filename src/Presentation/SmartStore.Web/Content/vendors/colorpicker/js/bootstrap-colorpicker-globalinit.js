;
(function ($) {
	var toxic = [
		"$",
		"red(",
		"green(",
		"blue(",
		"hsla(",
		"mix(",
		"hue(",
		"saturation(",
		"lightness(",
		"adjust-hue(",
		"lighten(",
		"darken(",
		"saturate(",
		"desaturate(",
	];

	function isValidColor(expr) {
		if (_.str.isBlank(expr))
			return true;

		if (expr.indexOf("$") > -1)
			return false;

		if (expr[0] == '#' || expr.startsWith("rgb(") || expr.startsWith("rgba(") || expr.startsWith("hsl(") || expr.startsWith("hsla("))
			return true;

		// Let pass all color names (red, blue etc.), but reject functions, e.g. "lighten(#fff, 10%)"
		return expr.indexOf("(") == -1;
	}

	var updateInput = $.colorpicker.prototype.updateInput;
	$.colorpicker.prototype.updateInput = function (val) {
		var expr = $(this.input).val();
		if (isValidColor(expr)) {
			updateInput.apply(this, { val });
		}
	};

	$(function () {
		$(document).on("keyup change input paste", ".sm-colorbox .form-control", function (e) {
			var el = $(this);
			if (!el.val() && el.attr('placeholder')) {
				el.parent().find('i').css('background-color', el.attr('placeholder'));
			}
		})
	});
}(jQuery));
