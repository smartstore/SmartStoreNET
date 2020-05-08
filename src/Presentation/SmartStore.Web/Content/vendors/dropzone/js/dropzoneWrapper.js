/*
* Dropzone Wrapper
*/

(function () {
	var dzOpts = Dropzone.prototype.defaultOptions;
	var resRoot = 'FileUploader.Dropzone.';

	dzOpts.dictDefaultMessage = Res[resRoot + 'DictDefaultMessage'];
	dzOpts.dictFallbackMessage = Res[resRoot + 'DictFallbackMessage'];
	dzOpts.dictFallbackText = Res[resRoot + 'DictFallbackText'];
	dzOpts.dictFileTooBig = Res[resRoot + 'DictFileTooBig'];
	dzOpts.dictInvalidFileType = Res[resRoot + 'DictInvalidFileType'];
	dzOpts.dictResponseError = Res[resRoot + 'DictResponseError'];
	dzOpts.dictCancelUpload = Res[resRoot + 'DictCancelUpload'];
	dzOpts.dictUploadCanceled = Res[resRoot + 'DictUploadCanceled'];
	dzOpts.dictCancelUploadConfirmation = Res[resRoot + 'DictCancelUploadConfirmation'];
	dzOpts.dictRemoveFile = Res[resRoot + 'DictRemoveFile'];
	dzOpts.dictMaxFilesExceeded = Res[resRoot + 'DictMaxFilesExceeded'];
})();

(function ($) {

	var assignableFiles = [];
	var assignableFileIds = "";
	var activeFiles = 0;
	var canUploadMoreFiles = true;

	$.fn.dropzoneWrapper = function (options) {
		return this.each(function () {
			var el = this, $el = $(this);

			var elDropzone = $el.closest('.dropzone-target'),
				fuContainer = $el.closest('.fileupload-container'),
				previewContainer = fuContainer.find(".preview-container");

			var elRemove = fuContainer.find('.remove'),
				elCancel = elDropzone.find('.cancel'),
				elFile = elDropzone.find('.fileinput-button'),
				elProgressBar = elDropzone.find('.progress-bar'),
				elStatusBar = elDropzone.find('.fileupload-status');

			// Init dropzone.
			elDropzone.addClass("dropzone");

			// File extensions of MediaManager are dotless but dropzone expects dots.
			var acceptedFiles = "";
			if ($el.data('accept')) {
				acceptedFiles = "." + $el.data('accept').replace(/\,/g, ",.");

				// Test
				//acceptedFiles += ",.mp4";
			}

			// Dropzone init params.
			var opts = {
				url: $el.data('upload-url'),
				//clickable: $el.find(".fileinput-button")[0],
				//clickable: elDropzone[0],
				clickable: elDropzone.find(".fu-message")[0],
				//autoQueue: false,
				//autoProcessQueue: false,
				parallelUploads: 1,
				uploadMultiple: true,
				acceptedFiles: acceptedFiles,
				maxFiles: options.maxFiles,
				previewsContainer: options.previewContainerId !== "" ? "#" + options.previewContainerId : null
			};

			// Place multifile upload preview into the designated spot defined by Media editor template.
			var previewTemplate;
			if (options.maxFiles > 1 && options.previewContainerId !== "") {
				previewTemplate = fuContainer.find(".file-preview-template");

				if (previewTemplate && previewTemplate.length !== 0) {
					opts.previewTemplate = previewTemplate[0].innerHTML;
				}
			}

			options = $.extend({}, opts, options);
			el = new Dropzone(fuContainer[0], options);

			//console.log(el);

			el.on("addedfile", function (file) {

				console.log("addedfile", file);

				// Reset progressbar when a new file was added.
				dzResetProgressBar(elProgressBar);
			});

			el.on("addedfiles", function (files) {

				console.log("addedfiles", files);

				elStatusBar.find(".total-file-count").text(files.length);

				// Reset progressbar when new files were added.
				dzResetProgressBar(elProgressBar);
			});

			el.on("processing", function (file) {

				var currentProcessingCount = el.getFilesWithStatus(Dropzone.PROCESSING).length;

				console.log("processing", currentProcessingCount);

				if (activeFiles === 0) {
					activeFiles = this.files.length;
				}
			});

			el.on("processingmultiple", function (file) {
				console.log("processingmultiple", file);
			});

			el.on("sending", function (file, xhr, formData) {

				console.log("sending");

				// Write user decision of duplicate handling into formdata before sending so it'll be sent to the server with each file upload.
				var enumId = fuContainer.data("dupe-handling-type");
				if (enumId) {
					formData.append("duplicateFileHandling", fuContainer.data("dupe-handling-type"));
				}

				if (options.onUploading) options.onUploading.apply(this, [file]);
			});

			el.on("sendingmultiple", function (file, xhr, formData) {
				console.log("sendingmultiple");
			});

			el.on("uploadprogress", function (file, percent, bytes) {

				console.log("uploadprogress", file, percent, bytes);

				// TODO: find better way to display status bar
				elStatusBar.removeClass("d-none");
				elStatusBar.find(".current-file").text(file.name);
			});

			el.on("totaluploadprogress", function (progress, totalBytes, totalBytesSent) {

				console.log("totaluploadprogress", progress, totalBytes, totalBytesSent);
				//console.log("getUploadingFiles:", this.getUploadingFiles().length);

				/*
				console.log("getAcceptedFiles:", this.getAcceptedFiles().length);
				console.log("getRejectedFiles:", this.getRejectedFiles().length);
				console.log("getQueuedFiles:", this.getQueuedFiles().length);
				console.log("getUploadingFiles:", this.getUploadingFiles().length);
				console.log("files:", this.files.length);
				*/

				elProgressBar
					.attr('aria-valuenow', progress)
					.css('width', progress + '%');

				elStatusBar.find(".percental-progress").text(Math.round(progress) + '%');

				// TODO: For picture uploads this is way too fast. Nothing can be seen (Though the console shows it works correct).
				elStatusBar.find(".current-file-count").text(activeFiles - this.getUploadingFiles().length);

				//console.log(activeFiles, this.getUploadingFiles().length, activeFiles - this.getUploadingFiles().length);
			});

			el.on("success", function (file, response, progress) {

				console.log("success", file, response, progress);

				// Only for singleupload.
				if (opts.maxFiles === 1) {

					//fuContainer.find('.fileupload-filename').html(file.name);
					//fuContainer.find('.fileupload-filesize').html(this.filesize(file.size));
					fuContainer.find('.fileupload-thumb').css('background-image', 'url("' + response.url + '")');
					fuContainer.find('.hidden').val(response.fileId).trigger('change');
					elRemove.show();
				}
				else {

					// If there was an error returned by the server set file status accordingly.
					if (!response.success) {
						file.status = Dropzone.ERROR;
						file.response = response;
					}
				}

				if (options.onUploadCompleted) options.onUploadCompleted.apply(this, [file, response, progress]);
			});

			el.on("successmultiple", function (files, response, progress) {

				console.log("successmultiple", files.length, response, progress);

				if (response.length) {
					$.each(response, function (i, value) {
						assignableFileIds += value.fileId + ",";
					});
				}
				else {
					assignableFileIds += response.fileId + ",";
				}

				assignableFiles = files;
			});

			el.on("complete", function (file) {
				console.log("complete", file);

				if (opts.maxFiles === 1) {
					// Reset dropzone for single file uploads, so other files can be uploaded again.
					this.removeAllFiles(true); 
				}
			});

			el.on("completemultiple", function (file, response, progress) {
				
				console.log("completemultiple", activeFiles, assignableFiles.length, assignableFileIds);

				var errorFiles = this.getFilesWithStatus(Dropzone.ERROR);
				var dupeFiles = errorFiles.filter(file => file.response.dupe === true);

				if (activeFiles === assignableFiles.length && dupeFiles.length === 0) {
					assignFilesToEntity(assignableFiles, assignableFileIds);
				}
				else {
					// Duplicate handling user decision wasn't done yet.
					assignableFileIds = "";
					assignableFiles = [];
				}
			});

			el.on("canceledmultiple", function (file, response, progress) {
				console.log("canceledmultiple");
			});

			el.on("queuecomplete", function () {
				console.log("queuecomplete");
				console.log("Status > getAcceptedFiles:", this.getAcceptedFiles().length);
				console.log("Status > getRejectedFiles:", this.getRejectedFiles().length);
				console.log("Status > ERROR:", this.getFilesWithStatus(Dropzone.ERROR).length);

				// Handle errors.
				var errorFiles = this.getFilesWithStatus(Dropzone.ERROR);
				var dupeFiles = errorFiles.filter(file => file.response.dupe === true);

				// TODO: these errors also need to be displayed (and maybe) somehow. 
				var otherErrors = errorFiles.filter(file => file.response.dupe === false);

				if (dupeFiles.length !== 0) {

					// Open duplicate file handler dialog
					SmartStore.Admin.Media.openDupeFileHandlerDialog(
						$el.data('dialog-url'),						// TODO: Place dialogUrl somewhere, where it can be accessed by openDuplicateHandlingDialog directly. 
						dupeFileHandlerCallback,		
						elDropzone.find(".fileupload").attr("id"),
						dupeFiles[0]								// Pass first conflicted file to function so it can be displayed.
					);
				}

				updateUploadStatus(this);
				$(".show-upload-summmary").show();
			});

			el.on("canceled", function (file) {
				if (options.onAborted) options.onAborted.apply(this, [file]);
			});

			el.on("removedfile", function (file) {

				// Reset progress bar when file was removed.
				dzResetProgressBar(elProgressBar);

				if (options.onFileRemove) options.onFileRemove.apply(this, [file]);
			});

			el.on("complete", function (file) {
				if (options.onCompleted) options.onCompleted.apply(this, [file]);
			});

			el.on("error", function (file, errMessage, xhr) {

				console.log(errMessage, file);

				if (xhr && file.status === "error") {
					console.log(xhr.statusText, "error");
				}

				// Single file only. Multifile errors must be shown in summary.
				if (!file.accepted && opts.maxFiles === 1) {
					displayNotification(errMessage, "error");
					//return;
				}

				if (options.onError) options.onError.apply(this, [file, errMessage]);
			});

			el.on("drop", function (files) {

				// Reset canUploadMoreFiles if new files have been added.
				canUploadMoreFiles = true;
			});

			el.on("maxfilesexceeded", function (file) {

				// Only for singleupload.
				if (opts.maxFiles === 1) {

					// Remove all files which may have been dropped for single uploads. Only accept the first file.
					if (canUploadMoreFiles) {
						this.removeAllFiles();
						this.addFile(file);
						canUploadMoreFiles = false;
					}
				}
			});

			function assignFilesToEntity(assignableFiles, assignableFileIds) {

				if ($el.data('assignment-url')) {

					$.ajax({
						async: false,
						cache: false,
						type: 'POST',
						url: $el.data('assignment-url'),
						data: {
							mediaFileIds: assignableFileIds,
							entityId: $el.data('entity-id')
						},
						success: function (response) {

							console.log("assignFilesToEntity", response, assignableFiles);

							$.each(response.response, function (i, value) {

								var file = assignableFiles.find(x => x.name.toLowerCase() === value.Name.toLowerCase());

								if (!file) {
									// Try get renamed file.
									var name = value.Name;
									var extension = name.substring(name.lastIndexOf("."), name.length);
									name = name.substring(0, name.lastIndexOf("-")) + extension;

									file = assignableFiles.find(x => x.name.toLowerCase() === name.toLowerCase());
								}

								if (file) {

									// set properties for newly added file preview
									var elPreview = $(file.previewElement);

									elPreview
										.attr("data-display-order", 1000)
										.attr("data-picture-id", value.MediaFileId)
										.attr("data-entity-picture-id", value.ProductMediaFileId)
										.attr("data-original-title", '<div class="text-left px-3"><em>' + file.name + '</em> <br/> <b>' + el.filesize(file.size) + '</b></div>')
										.addClass("d-flex")
										.removeClass("d-none")
										.tooltip();

									elPreview
										.find('img')
										.attr('src', file.dataUrl);

									previewContainer.append(elPreview);
								}
								else {
									console.log("Error when adding preview element.", value.Name.toLowerCase());
								}
							});
						}
					});
				}
			}

			// Deleting.
			$(fuContainer).on("click", ".delete-entity-picture", function (e) {

				var previewThumb = $(this).closest('.dz-image-preview');

				$.ajax({
					async: false,
					cache: false,
					type: 'POST',
					url: $el.data('remove-url'),
					data: { id: previewThumb.data("entity-picture-id") },
					success: function () {
						previewThumb.removeClass("d-flex")
							.addClass("d-none");

						// TODO: Files must be removed from dropzone.
						//dropzone.removeFile(file);
						console.log("On Deleting", el, el.files);
					}
				});

				return false;
			});

			elFile.on('click', function (e) {

				// Reset canUploadMoreFiles if a new file is about to be selected via FileOpen dialog.
				canUploadMoreFiles = true;
			});

			// Remove uploaded file (single upload only).
			elRemove.on('click', function (e) {

				e.preventDefault();

				fuContainer.find('.fileupload-thumb').css('background-image', 'url("' + $el.data('fallback-url') + '")');
				fuContainer.find('.hidden').val(0).trigger('change');

				$(this).hide();

				if (options.onFileRemove)
					options.onFileRemove.apply(this, [e, el]);

				return false;
			});

			// If multiupload > display tooltips for preview images in preview container.
			if (opts.maxFiles !== 1) {
				$(".dz-image-preview").tooltip();
			}
		});
	};

	// Global events

	// Used to highlight dropzone when a file is dragged into the browser.
	var fuContainer = $('.fileupload-container');

	fuContainer.on("dragover", function (e) {

		if (fuContainer.hasClass("dz-highlight"))
			return;

		fuContainer.addClass("dz-highlight");

	}).on("dragleave", function (e) {
		if ($(e.relatedTarget).closest('.fileupload-container').length === 0) {

			if (!fuContainer.hasClass("dz-highlight"))
				return;

			fuContainer.removeClass("dz-highlight");
		}
	}).on("drop", function (e) {
		if (!fuContainer.hasClass("dz-highlight"))
			return;

		fuContainer.removeClass("dz-highlight");
	});

	$(document).on("drag", ".dz-image-preview", function (e) {
		$(".dz-image-preview").tooltip("hide").trigger('mouseleave');
	});

	$(document).on("click", ".show-upload-summmary", function (e) {

		alert("Not implemented yet!");

		//$(".fileupload-status").show();

		return false;
	});

	// Callback function for duplicate file handling dialog.
	function dupeFileHandlerCallback(userInput, saveSelection) {

		var duplicateDialog = $("#duplicate-window");
		var callerId = duplicateDialog.data("caller-id");

		// Store user decision where it can be accessed by other events (e.g. sending).
		$("#" + callerId)
			.closest(".fileupload-container")
			.attr("data-dupe-handling-type", userInput);

		var dropzone = Dropzone.forElement($("#" + callerId).closest(".fileupload-container")[0]);
		var errorFiles = dropzone.getFilesWithStatus(Dropzone.ERROR);

		// Get all duplicate files.
		var dupeFiles = errorFiles.filter(file => file.response.dupe === true);

		if (!saveSelection) {
			var firstFile = dupeFiles[0];

			// Reset file status.
			resetFileStatus(firstFile);

			// Process first file. 
			dropzone.processFile(firstFile);

			// If current file is last file > close dialog else display next file.
			if (dupeFiles.length === 1) {
				duplicateDialog.modal('hide');
			}
			else {
				SmartStore.Admin.Media.displayDuplicateFileInDialog(dupeFiles[1]);
			}

			// And leave.
			return;
		}
		else {
			// Process all files and leave.
			dropzone.processFiles(dupeFiles);
			duplicateDialog.modal('hide');
		}
	}

	function updateUploadStatus(dropzone) {
		var cntStatus = $(".fileupload-status"),
			summary = cntStatus.find(".fileupload-status-summary"),
			uploadedFileCount = summary.find(".uploaded-file-count"),
			totalFileCount = summary.find(".total-file-count"),
			errors = cntStatus.find(".erroneous-files"),
			updated = cntStatus.find(".updated-files"),
			replaced = cntStatus.find(".replaced-files"),
			uploaded = cntStatus.find(".uploaded-files");

		var errorFiles = dropzone.getFilesWithStatus(Dropzone.ERROR);
		var dupeFiles = errorFiles.filter(file => file.response.dupe === true);
		var otherErrors = errorFiles.filter(file => file.response.dupe === false);

		// TODO: finish the job ;-)
	}

	function resetFileStatus(file) {
		if (file.status === Dropzone.SUCCESS) {
			file.status = undefined;
			file.accepted = undefined;
			file.processing = false;
		}
	}

	function dzResetProgressBar(elProgressBar) {

		_.delay(function () {
			// Remove transition for reset.
			elProgressBar.css("transition", "none");

			elProgressBar
				.attr('aria-valuenow', 0)
				.css('width', 0 + '%');

			_.delay(function () {
				// Remove inline transition style after transition (0.25s) was performed.
				elProgressBar.css("transition", "");
			}, 250);

		}, 300);
	}

})(jQuery);

