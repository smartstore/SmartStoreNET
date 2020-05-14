
SmartStore.Admin.Media = (function () {
	var dupeFileHandlerDialog = $("#duplicate-window");
	var dupeFileDisplay = dupeFileHandlerDialog.find(".dupe-file-display");
	var _file;

	function initDialog(fuContainer, firstDupe, callback, callerId) {
		// Get dialog via ajax and append to body.
		$.ajax({
			async: true,
			cache: false,
			type: 'POST',
			url: fuContainer.find(".fileupload").data('dialog-url'),
			success: function (response) {
				$("body").append($(response));
				dupeFileHandlerDialog = $("#duplicate-window");
				dupeFileDisplay = dupeFileHandlerDialog.find(".dupe-file-display");

				// Display first duplicate.
				refreshDialog(firstDupe);
				_file = firstDupe;

				// Open dialog.
				dupeFileHandlerDialog.modal('show');

				// Reset user selection on closing so dialog can open for the next queue.
				dupeFileHandlerDialog.on('hidden.bs.modal', function () {
					fuContainer.data("dupe-handling-type", "");
				});

				// Listen to change events of radio group (dupe handling type) and display name of renamed file accordingly.
				$(document).on("change", dupeFileHandlerDialog.find('input[name=dupe-handling-type]'), function (e) {
					var fileName = _file.name;

					if ($(e.target).val() === "2") {
						var newPath = _file.response.newPath;
						fileName = newPath.substr(newPath.lastIndexOf("/") + 1);
					}
					
					dupeFileDisplay.find(".file-name").text(fileName);
				});

			}
		});

		// User has made a decision.
		$(document).on("click", "#start-upload", function () {
			var dupeFileHandlingType = dupeFileHandlerDialog.find('input[name=dupe-handling-type]:checked').val();
			var saveSelection = dupeFileHandlerDialog.find('#save-selection').is(":checked");

			// Store user decision where it can be accessed by other events (e.g. dropzone > sending).
			fuContainer.data("dupe-handling-type", dupeFileHandlingType);

			if (callback)
				callback.apply(this, [dupeFileHandlingType, saveSelection, callerId]);
		});

		$(document).on("click", "#cancel-upload", function () {

			// TODO: all pending files need to removed from dropzone!!!

			dupeFileHandlerDialog.modal('hide');
		});
	}

	function refreshDialog(file) {
		var existingFileDisplay = dupeFileHandlerDialog.find(".existing-file-display");
		var formatedDateFile = moment(file.lastModifiedDate).format('L LTS');

		// Display current filename in intro text.
		dupeFileHandlerDialog.find(".intro .current-file").text(file.name);

		// Display uploaded file.
		dupeFileDisplay.find(".file-img").attr("src", file.dataURL);
		dupeFileDisplay.find(".file-name").text(file.name);
		dupeFileDisplay.find(".file-date").text(formatedDateFile);
		dupeFileDisplay.find(".file-size").text(_.formatFileSize(file.size));

		// TODO: What happens when uploading e.g. documents
		if (file.width && file.height) {
			dupeFileDisplay.find(".file-dimensions").text(file.width + " x " + file.height);
		}

		// Display existing file.
		existingFileDisplay.find(".file-img").attr("src", file.response.url);
		existingFileDisplay.find(".file-name").text(file.name);		// No need for writing the name of the existing file into the response. We know its the same as the uploaded file.
		existingFileDisplay.find(".file-date").text(file.response.date);
		existingFileDisplay.find(".file-dimensions").text(file.response.dimensions);
		existingFileDisplay.find(".file-size").text(_.formatFileSize(file.response.size));
    }

	return {
		openDupeFileHandlerDialog: function (callback, callerId, firstDupe) {
			var fuContainer = $("#" + callerId).closest(".fileupload-container");

			if (dupeFileHandlerDialog.length === 0) {
				initDialog(fuContainer, firstDupe, callback, callerId);
			}
			else {
				// Display first duplicate.
				refreshDialog(firstDupe);

				// Open dialog.
				dupeFileHandlerDialog.modal('show');
			}

			return {
				get file() {
					return _file;
				},
				set file(value) {
					_file = value;
					refreshDialog(value);
				}
			};
		}
	};
})();
