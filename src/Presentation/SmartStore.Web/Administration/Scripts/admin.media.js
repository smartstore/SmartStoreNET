
SmartStore.Admin.Media = (function () {

	function initDialog() {
		//
	}

	function refreshDialog(file) {
		//
    }

	return {
		openDupeFileHandlerDialog: function (callback, callerId, firstDupe) {
			var dupeFileHandlerDialog = $("#duplicate-window");
			var fuContainer = $("#" + callerId).closest(".fileupload-container");

			if (dupeFileHandlerDialog.length === 0) {

				// Get dialog via ajax and append to body.
				$.ajax({
					async: false,
					cache: false,
					type: 'POST',
					url: fuContainer.find(".fileupload").data('dialog-url'),
					success: function (response) {

						$("body").append($(response));
						dupeFileHandlerDialog = $("#duplicate-window");

						// Display first duplicate.
						SmartStore.Admin.Media.displayDuplicateFileInDialog(firstDupe);

						// Open dialog.
						dupeFileHandlerDialog.modal('show');
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
					dupeFileHandlerDialog.modal('hide');
				});
			}
			else {
				// TODO: 3 lines of duplicate code see line 20 ff DRY???
				// Display first duplicate.
				SmartStore.Admin.Media.displayDuplicateFileInDialog(firstDupe);
				fuContainer.find("#duplicate-conflict-status .current-file").text(firstDupe.name);

				// Open dialog.
				dupeFileHandlerDialog.modal('show');
			}

			dupeFileHandlerDialog.on('hidden.bs.modal', function (e) {

				console.log("hidden.bs.modal", $("#" + callerId).closest(".fileupload-container"));

				// Reset user selection so dialog can open for the next queue.
				fuContainer.data("dupe-handling-type", "");
			});

			var _file;
			return {
				get file() {
					return _file;
				},
				set file(value) {
					// Do something
					_file = value;
					refreshDialog(value);
				}
			}
		},

		displayDuplicateFileInDialog: function (file) {
			var dupeFileHandlerDialog = $("#duplicate-window");
			var dupeFileDisplay = dupeFileHandlerDialog.find(".dupe-file-display");
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

			// Listen to change events of radio button and display name of renamed file accordingly.
			$(document).on("change", dupeFileHandlerDialog.find('input[name=dupe-handling-type]'), function (e) {

				var currentSelection = $(e.target).val();

				if (currentSelection === "2") {

					var newPath = file.response.newPath;
					var fileName = newPath.substr(newPath.lastIndexOf("/") + 1);

					// Display renamed filename.
					dupeFileDisplay.find(".file-name").text(fileName);
				}
				else {
					// Display original filename.
					dupeFileDisplay.find(".file-name").text(file.name);
				}
			});
		}
	};
})();
