
SmartStore.Admin.Media = {
	initDuplicateHandlingDialog: function (dialogUrl, callback) {
		var duplicateDialog = $("#duplicate-window");

		if (duplicateDialog.length === 0) {

			// Get dialog via ajax and append to body.

			$.ajax({
				async: false,
				cache: false,
				type: 'POST',
				url: dialogUrl,
				success: function (response) {
					$("body").append($(response));
				}
			});

			$(document).one("click", "#accept-selected", function () {
				if (callback) callback.apply();
			});
		}
	}
};
