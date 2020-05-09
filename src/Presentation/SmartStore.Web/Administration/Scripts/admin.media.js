
SmartStore.Admin.Media = {
	openDupeFileHandlerDialog: function (dialogUrl, callback, callerId, firstDupe) {
		var dupeFileHandlerDialog = $("#duplicate-window");

		if (dupeFileHandlerDialog.length === 0) {

			// Get dialog via ajax and append to body.

			$.ajax({
				async: false,
				cache: false,
				type: 'POST',
				url: dialogUrl,
				success: function (response) {
					$("body").append($(response));
					dupeFileHandlerDialog = $("#duplicate-window");

					// display firstDupe
					SmartStore.Admin.Media.displayDuplicateFileInDialog(firstDupe);

					// TODO: just pass the id to the callback function.
					// Set dz caller id to identify dropzone in outside events.
					dupeFileHandlerDialog.attr("data-caller-id", callerId);
					dupeFileHandlerDialog.modal('show');
				}
			});

			// TODO: Listen to change events of radio button and display name of first file accordingly
			$(document).on("change", dupeFileHandlerDialog.find('input[name=dupe-handling-type]'), function (e) {

				var currentSelection = $(e.target).val();
				var btn = dupeFileHandlerDialog.find("#start-upload");
				var btnRes = "FileUploader.DuplicateDialog.Btn.Replace";

				switch (currentSelection) {
					case "0":
						btnRes = "FileUploader.DuplicateDialog.Btn.Skip";
						break;
					case "1":
						btnRes = "FileUploader.DuplicateDialog.Btn.Replace";
						break;
					case "2":
						btnRes = "FileUploader.DuplicateDialog.Btn.Rename";
						break;
					default:
				}

				btn.text(Res[btnRes]);
			});

			// User has made a decision.
			$(document).on("click", "#start-upload", function () {
				var duplicateHandlingType = dupeFileHandlerDialog.find('input[name=dupe-handling-type]:checked').val();
				var saveSelection = dupeFileHandlerDialog.find('#save-selection').is(":checked");

				if (callback)
					callback.apply(this, [duplicateHandlingType, saveSelection]);
			});

			$(document).on("click", "#cancel-upload", function () {
				dupeFileHandlerDialog.modal('hide');
			});
		}
	}, 

	displayDuplicateFileInDialog: function (file) {
		var dupeFileHandlerDialog = $("#duplicate-window");
		var cntComparison = dupeFileHandlerDialog.find(".dupe-media-comparison");
		var dupeFileDisplay = cntComparison.find(".dupe-file-display");
		var existingFileDisplay = cntComparison.find(".existing-file-display");

		// TODO: Renamed file must be written into reponse, so it can be displayed when user switches options.
		/*
		cntComparison.attr("original-file-name", file.response.name);
		cntComparison.attr("renamed-file-name", "");
		console.log(file);
		*/

		var formatedDateFile = moment(file.lastModifiedDate).format('L LTS');

		dupeFileDisplay.find(".file-img").attr("src", file.dataURL);
		dupeFileDisplay.find(".file-name").text(file.name);
		dupeFileDisplay.find(".file-date").text(formatedDateFile);
		dupeFileDisplay.find(".file-size").text(_.formatFileSize(file.size));

		// TODO: What happens when uploading e.g. documents
		if (file.width && file.height) {
			dupeFileDisplay.find(".file-dimensions").text(file.width + " x " + file.height);
		}

		existingFileDisplay.find(".file-img").attr("src", file.response.url);
		existingFileDisplay.find(".file-name").text(file.name);		// No need for writing the name of the existing file into the response. We know its the same as the uploaded file.
		existingFileDisplay.find(".file-date").text(file.response.date);
		existingFileDisplay.find(".file-dimensions").text(file.response.dimensions);
		existingFileDisplay.find(".file-size").text(_.formatFileSize(file.response.size));
	}
};
