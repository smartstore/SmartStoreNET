var Hacks = {
	Telerik: {
		// TODO: temp > Handle Button with MVC
		handleButton: function (el) {
			el.each(function () {
				var btn = $(this);
				if (btn.hasClass("t-grid-add") && btn.find('> .fa').length === 0) {
					btn.prepend('<i class="fa fa-plus mr-2"></i>');
				}
				else if (btn.hasClass("t-grid-save-changes") && btn.find('> .fa').length === 0) {
					btn.prepend('<i class="fa fa-check mr-2"></i>');
				}
			});
		}
	}
};