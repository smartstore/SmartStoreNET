/*
* Dropzone Wrapper
*/

Dropzone.prototype.defaultOptions.dictDefaultMessage = Res['FileUploader.Dropzone.DictDefaultMessage'];
Dropzone.prototype.defaultOptions.dictFallbackMessage = Res['FileUploader.Dropzone.DictFallbackMessage'];
Dropzone.prototype.defaultOptions.dictFallbackText = Res['FileUploader.Dropzone.DictFallbackText'];
Dropzone.prototype.defaultOptions.dictFileTooBig = Res['FileUploader.Dropzone.DictFileTooBig'];
Dropzone.prototype.defaultOptions.dictInvalidFileType = Res['FileUploader.Dropzone.DictInvalidFileType'];
Dropzone.prototype.defaultOptions.dictResponseError = Res['FileUploader.Dropzone.DictResponseError'];
Dropzone.prototype.defaultOptions.dictCancelUpload = Res['FileUploader.Dropzone.DictCancelUpload'];
Dropzone.prototype.defaultOptions.dictUploadCanceled = Res['FileUploader.Dropzone.DictUploadCanceled'];
Dropzone.prototype.defaultOptions.dictCancelUploadConfirmation = Res['FileUploader.Dropzone.DictCancelUploadConfirmation'];
Dropzone.prototype.defaultOptions.dictRemoveFile = Res['FileUploader.Dropzone.DictRemoveFile'];
Dropzone.prototype.defaultOptions.dictMaxFilesExceeded = Res['FileUploader.Dropzone.DictMaxFilesExceeded'];

(function ($) {
	var remainingFiles;
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
				var enumId = fuContainer.data("duplicate-handling");
				if (enumId) {
					formData.append("duplicateFileHandling", fuContainer.data("duplicate-handling"));
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

					// If there was an error returned by the server.
					if (!response.success) {

						if (fuContainer.data("duplicate-handling") === undefined) {

							// Duplicate handling user input is unset.

							SmartStore.Admin.Media.initDuplicateHandlingDialog($el.data('dialog-url'), duplicateFileHandlingCallback);

							var duplicateDialog = $("#duplicate-window");

							// Set dz caller id to identify dropzone in outside events.
							duplicateDialog.attr("data-caller-id", elDropzone.find(".fileupload").attr("id"));

							// Ask user once what he wants to do with conflicting files.
							duplicateDialog.modal('show');

							// Store all remaining files to be able to add them again after duplicate handling by user decision.
							remainingFiles = this.getActiveFiles();

							// Add current file to remaining files.
							remainingFiles.push(file);

							// Copy into new array.
							remainingFiles = remainingFiles.slice(0, remainingFiles.length);

							// Remove all files to break the upload chain. They'll be added again in duplicate handling dialog click event.
							this.removeAllFiles();
						}
						else {

							// Duplicate handling user input is set.
							
							// File was rejected by the server thus remove it from dropzone.
							this.removeFile(file);
						}
					}
					else {
						// Picture wasn't uploaded yet.
						// TODO: If this case won't be needed by the end of development > write different if clauses.
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

				if (activeFiles === assignableFiles.length) {
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

			el.on("queuecomplete", function (file) {
				console.log("queuecomplete");

				console.log("Status > getAcceptedFiles:", this.getAcceptedFiles().length);
				console.log("Status > getRejectedFiles:", this.getRejectedFiles().length);
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

				console.log("assignFilesToEntity", assignableFileIds);

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

									//console.log("pop", file);
									//assignableFiles.pop(file);
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

	// Callback fdunction for duplicate file handling dialog.
	function duplicateFileHandlingCallback() {

		var duplicateDialog = $("#duplicate-window");

		// Should never happen.
		if (!remainingFiles) {
			duplicateDialog.modal('hide');
			return;
		}

		var callerId = duplicateDialog.data("caller-id");
		var userInput = duplicateDialog.find('input[name=duplicate-handling]:checked').data("enum-id");

		// Store user decision for application at later conflicts.
		$("#" + callerId)
			.closest(".fileupload-container")
			.attr("data-duplicate-handling", userInput);

		var dropzone = Dropzone.forElement($("#" + callerId).closest(".fileupload-container")[0]);

		// Set status for remainingItems.
		$.each(remainingFiles, function (i, file) {

			if (file.status === Dropzone.SUCCESS) {
				file.status = undefined;
				file.accepted = undefined;
				file.processing = false;
			}

			dropzone.addFile(file);
		});

		console.log("DialogClosed > remainingFiles", remainingFiles);

		dropzone.processFiles(remainingFiles);

		remainingFiles = [];

		duplicateDialog.modal('hide');
	}


})(jQuery);

