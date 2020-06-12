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
	var canUploadMoreFiles = true;	// TODO: investigate!!! This can be done better.
	var dialog = SmartStore.Admin.Media.fileConflictResolutionDialog;
	var file;

	$.fn.dropzoneWrapper = function (options) {
		return this.each(function () {
			var el = this, $el = $(this);

			var elDropzone = $el.closest('.dropzone-target'),
				fuContainer = $el.closest('.fileupload-container');

			if (!fuContainer.length) {
				$el.closest('.dropzone-container').wrap('<div class="fileupload-container h-100"></div>');
				fuContainer = $el.closest('.fileupload-container');
			}

			var elRemove = fuContainer.find('.remove'),
				elProgressBar = fuContainer.find('.progress-bar'),
				elStatus = fuContainer.find('.fileupload-status'),
				elStatusWindow = $(".fu-status-window"),
				previewContainer = fuContainer.find(".preview-container"),
				elCancel = fuContainer.find('.cancel');

			var displayPreviewInList = previewContainer.data("display-list-items");

			// Init dropzone.
			elDropzone.addClass("dropzone");

			// Dropzone init params.
			var opts = {
				url: $el.data('upload-url'),
				//clickable: elDropzone[0],
				clickable: options.clickable ? options.clickable : elDropzone.find(".fu-message")[0],
				//autoQueue: false,
				//autoProcessQueue: false,
				parallelUploads: 1,
				uploadMultiple: true,
				acceptedFiles: $el.data('accept'),
				maxFiles: options.maxFiles,
				maxFilesize: Math.round(options.maxFilesSize / 1024),
				previewsContainer: options.previewContainerId !== "" ? "#" + options.previewContainerId : null
			};

			// Place multifile upload preview into the designated spot defined by Media editor template.
			var previewTemplate;

			if (options.maxFiles > 1 && options.previewContainerId !== "") {
				if (displayPreviewInList)
					previewTemplate = fuContainer.find(".file-preview-template-list");
				else 
					previewTemplate = fuContainer.find(".file-preview-template");

				if (previewTemplate && previewTemplate.length !== 0) 
					opts.previewTemplate = previewTemplate[0].innerHTML;
			}

			// If multifile > display tooltips for preview images in preview container.
			if (opts.maxFiles !== 1) {
				$(".dz-image-preview").tooltip();
			}
			else {
				// SingleFile: If there's no file, there's no remove button.
				var currentFileId = fuContainer.find('.hidden').val();

				if ((!currentFileId || currentFileId == 0) || !options.showRemoveButton)
					elRemove.hide();
				else {
					elRemove.show();
				}
			}

			// Init sorting  if preview items aren't displayed in a list.
			if (!displayPreviewInList && options.maxFiles > 1) {
				previewContainer.sortable({
					items: fuContainer.find('.dz-image-preview'),
					handle: '.fu-file-dragarea',
					ghostClass: 'sortable-ghost',
					animation: 150
				}).on('sort', function (e, ui) {
					sortMediaFiles();
				});
			}
			
			options = $.extend({}, opts, options);
			el = new Dropzone(fuContainer[0], options);

			el.on("addedfile", function (file) {
				logEvent("addedfile", file);

				setPreviewIcon(file, displayPreviewInList);

				if (displayPreviewInList) {
					var progress = window.createCircularSpinner(36, true, 6, null, null, true, true, true);
					$(file.previewTemplate).find(".upload-status").append(progress);
				}
				
				// If file is a duplicate prevent it from being displayed in preview container.
				if (preCheckForDuplicates(file.name, previewContainer)) {
					$(file.previewTemplate).addClass("d-none");
				}
			});

			el.on("addedfiles", function (files) {
				logEvent("addedfiles", files);
				if (Array.isArray(files))
					activeFiles = files.filter(file => file.accepted === true).length;

				// Reset former decision (maybe better placed in onCompletedHandler)
				fuContainer.data("resolution-type", "");

				// Status
				if (elStatusWindow.length > 0) {
					elStatusWindow.find(".current-file-count").text(files.length);
					elStatusWindow.find(".current-file-text").text(Res['FileUploader.StatusWindow.Uploading.File' + (files.length === 1 ? "" : "s")]);
					elStatusWindow.find(".flyout-commands").addClass("show");
					elStatusWindow.attr("data-upload-in-progress", true);
				}
			});

			el.on("processing", function (file) {
				var currentProcessingCount = el.getFilesWithStatus(Dropzone.PROCESSING).length;

				logEvent("processing", file, currentProcessingCount);

				// Data attribute can be altered by MediaManager to specify the designated media folder.
				this.options.url = $el.data("upload-url");
			});

			el.on("processingmultiple", function (files) {
				logEvent("processingmultiple", files);
			});

			el.on("sending", function (file, xhr, formData) {
				logEvent("sending", file, xhr, formData);

				// Write user decision of duplicate handling into formdata before sending so it'll be sent to the server with each file upload.
				var enumId = fuContainer.data("resolution-type");
				if (enumId) {
					formData.append("duplicateFileHandling", enumId);
				}

				// Send type filter if set.
				var typeFilter = $el.data('type-filter');
				if (typeFilter) {
					if (formData.has("typeFilter"))
						formData.delete("typeFilter");

					for (var type of $el.data('type-filter').split(",")) {
						formData.append("typeFilter", type);
					}
				}
				
				if (options.onUploading) options.onUploading.apply(this, [file]);
			});

			el.on("sendingmultiple", function (files, xhr, formData) {
				logEvent("sendingmultiple", files, xhr, formData);
			});

			el.on("uploadprogress", function (file, progress, bytes) {
				logEvent("uploadprogress", file, progress, bytes);

				if (opts.maxFiles === 1) {
					// Singlefile.
					elProgressBar.attr('aria-valuenow', progress).css('width', progress + '%');
				}
				else {
					// Mulifile.
					if (!displayPreviewInList) {
						var fileProgressBar = $(file.previewTemplate).find(".progress-bar");
						fileProgressBar.attr('aria-valuenow', progress).css('width', progress + '%');
					}
					else {
						window.setCircularProgressValue($(file.previewTemplate), progress);
					}
				}
			});

			el.on("totaluploadprogress", function (progress, totalBytes, totalBytesSent) {
				logEvent("totaluploadprogress", progress, totalBytes, totalBytesSent);
			});

			el.on("success", function (file, response, progress) {
				logEvent("success", file, response, progress);

				if (opts.maxFiles === 1) {
					displaySingleFilePreview(response, fuContainer, options);
				}
				else {
					// Multifile
					// If there was an error returned by the server set file status accordingly.
					if (response.length) {
						for (fileResponse of response) {
							if (!fileResponse.success) {
								file.status = Dropzone.ERROR;
								file.media = fileResponse;
							}
						}
					}
					else if (!response.success) {
						file.status = Dropzone.ERROR;
						file.media = response;
					}
					else {
						file.media = response;
					}

					if (displayPreviewInList) {
						var template = $(file.previewTemplate);
						template.removeClass("dz-image-preview");
						var icon = template.find(".upload-status > i");
						icon.removeClass("d-none");
						template.find(".circular-progress").addClass("d-none");
					}
				}

				if (options.onUploadCompleted) options.onUploadCompleted.apply(this, [file, response, progress]);
			});

			el.on("successmultiple", function (files, response, progress) {
				logEvent("successmultiple", files, response, progress);

				if (opts.maxFiles === 1)
					return;

				if (response.length) {
					$.each(response, function (i, value) {
						assignableFileIds += value.id + ",";
					});
				}
				else {
					assignableFileIds += response.id + ",";
				}

				if (files.length) {
					assignableFiles = files;
				}
				else {
					assignableFiles.push(files);
				}
			});

			el.on("complete", function (file) {
				logEvent("complete", file);

				if (opts.maxFiles === 1) {
					// Reset dropzone for single file uploads, so other files can be uploaded again.
					this.removeAllFiles(true); 
				}

				//if (options.onUploadCompleted) options.onUploadCompleted.apply(this, [file]);
			});

			el.on("completemultiple", function (files) {
				logEvent("completemultiple", files);
				logEvent("completemultiple", " > activeFiles, assignableFiles.length, assignableFileIds", activeFiles, assignableFiles.length, assignableFileIds);

				var dupeFiles = this.getFilesWithStatus(Dropzone.ERROR)
					.filter(file => file.media && file.media.dupe === true);

				// Dupe file handling is 'replace' thus no need for assignment to entity (media IDs remain the same, while file was altered). 
				if (fuContainer.data("resolution-type") === 1) {
					// Update preview pic of replaced media file.
                    for (var newFile of files) {
                        var elCurrentFile = previewContainer.find(".dz-image-preview[data-media-id='" + newFile.media.id + "']");
						elCurrentFile.find("img").attr("src", newFile.dataURL);

						this.removeFile(newFile);
					}
				}

				if (activeFiles === assignableFiles.length && dupeFiles.length === 0) {
					assignFilesToEntity(assignableFiles, assignableFileIds);
					// Has to be done after success of assignFilesToEntity
					//sortMediaFiles();
				}
				else {
					// Duplicate handling user decision wasn't done yet.
					assignableFileIds = "";
					assignableFiles = [];
				}
			});

			el.on("canceledmultiple", function (files) {
				logEvent("canceledmultiple", files);
			});

			el.on("queuecomplete", function () {
				logEvent("queuecomplete");

				var dupeFiles = this.getFilesWithStatus(Dropzone.ERROR)
					.filter(file => file.media && file.media.dupe === true);
				var successFiles = this.getFilesWithStatus(Dropzone.SUCCESS);

				// If there are duplicates & dialog isn't already open > open duplicate file handler dialog.
				if (dupeFiles.length !== 0 && !dialog.isOpen) {

					// Close confirmation dialog. User was to slow. Uploads are complete.
					if (elStatusWindow.data("confirmation-requested")) {
						$("#modal-confirm-shared").modal("hide");
					}

					// Open duplicate file handler dialog.
					dialog.open({
						queue: SmartStore.Admin.Media.convertDropzoneFileQueue(dupeFiles),
						callerId: elDropzone.find(".fileupload").attr("id"),
						onResolve: dupeFileHandlerCallback,
						onComplete: dupeFileHandlerCompletedCallback
					});
				}

				updateUploadStatus(this, elStatus);

				// Status
				if (elStatusWindow.length > 0) {
					elStatusWindow.find(".current-file-count").text(successFiles.length);
					elStatusWindow.find(".current-file-text").text(Res['FileUploader.StatusWindow.Complete.File' + (successFiles.length === 1 ? "" : "s")]);
					elStatusWindow.find(".flyout-commands").removeClass("show");
					elStatusWindow.attr("data-upload-in-progress", false);
				}

				// Reset progressbar when queue is complete.
				if (opts.maxFiles === 1) {
					// SingleFile
					dzResetProgressBar(elProgressBar);
				}
				else if (!displayPreviewInList || (displayPreviewInList && dupeFiles.length !== 0)) {		// Don't reset progress bar for status window if dupefiles = 0
					// MultiFile
					var uploadedFiles = this.files;
					
					for (file of uploadedFiles) {
						// Only reset progress bar if there was an error (file is dupe) and the files must be processed again.
						if (file.status === Dropzone.ERROR) {
							dzResetProgressBar($(file.previewElement).find(".progress-bar"));
						}
					}
				}

				if (options.onCompleted) options.onCompleted.apply(this, [successFiles]);

				// DEV
				//$(".open-upload-summmary").show();
			});

			el.on("canceled", function (file) {
				logEvent("canceled", file);
				if (options.onAborted) options.onAborted.apply(this, [file]);
			});

			el.on("canceledmultiple", function (file) {
				logEvent("canceledmultiple", file);
			});

			el.on("removedfile", function (file) {
				logEvent("removedfile", file);

				// Reset progress bar when file was removed.
				dzResetProgressBar(elProgressBar);

				// Apply remove event only on explicit user interaction via remove button.
                //if (options.onFileRemove) options.onFileRemove.apply(this, [file]);
			});

			el.on("error", function (file, errMessage, xhr) {
				logEvent("error", file, errMessage, xhr);

                if (errMessage && !_.isEmpty(errMessage.message)) {
                    errMessage = errMessage.message;
                }

				// Write current message into file so it can be displayed in file upload status.
				file.errMessage = errMessage;

				if (xhr && file.status === "error") {
					console.log(xhr.statusText, "error");
				}

				displayNotification(errMessage, "error");
				this.removeFile(file);
				
				if (options.onError) options.onError.apply(this, [file, errMessage]);
			});

			el.on("errormultiple", function (files, errMessage) {
				logEvent("errormultiple", files, errMessage);
			});
			
			el.on("drop", function (files) {
				logEvent("drop", files);
				// Reset canUploadMoreFiles if new files have been added.
				canUploadMoreFiles = true;
			});

			el.on("maxfilesexceeded", function (file) {
				logEvent("maxfilesexceeded", file);

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
				if ($el.data('assignment-url') &&
					$el.data('entity-id') &&
					assignableFileIds !== "" &&
					assignableFiles.length > 0) {
					$.ajax({
						async: true,
						cache: false,
						type: 'POST',
						url: $el.data('assignment-url'),
						data: {
							mediaFileIds: assignableFileIds,
							entityId: $el.data('entity-id')
						},
						success: function (response) {

							$.each(response.response, function (i, value) {
								var file = assignableFiles.find(x => x.media.id === value.MediaFileId);

								if (!file) {
									// Try get renamed file.
									var name = value.Name;
									var extension = name.substring(name.lastIndexOf("."), name.length);
									name = name.substring(0, name.lastIndexOf("-")) + extension;

									file = assignableFiles.find(x => x.name.toLowerCase() === name.toLowerCase());
								}

								if (file) {
									// Set properties for newly added file preview.
									//var elPreview = file.previewElement ? $(file.previewElement) : $(fuContainer.find(".file-preview-template").html());
                                    var elPreview = file.previewElement ? $(file.previewElement) : $(previewTemplate.html());

									elPreview
										.attr("data-display-order", 1000)
										.attr("data-media-id", value.MediaFileId)
										.attr("data-media-name", value.Name)
										.attr("data-entity-media-id", value.ProductMediaFileId)
										.removeClass("d-none");

									elPreview.find(".fu-file-info-name").html(file.name);

                                    elPreview
                                        .find('img')
                                        .attr('src', file.dataURL || file.media.thumbUrl);

									previewContainer.append(elPreview);
								}
								else {
									console.log("Error when adding preview element.", value.Name.toLowerCase());
								}
							});

							sortMediaFiles();
						}
					});
				}
			}

			function sortMediaFiles() {
				if ($el.data('sort-url') && $el.data('entity-id')) {
					var items = previewContainer.find('.dz-image-preview');

					var newOrder = [];
					$.each(items, function (i, val) {
						newOrder.push($(val).data('entity-media-id'));
					});

					// Set display order of ProductPicture.
					$.ajax({
						async: true,
						cache: false,
						type: 'POST',
						url: $el.data('sort-url'),
						data: {
							pictures: newOrder.join(","),
							entityId: $el.data('entity-id')
						},
						success: function (response) {
							// Set EntityMediaId & current DisplayOrder.
							$.each(response.response, function (index, value) {
								var preview = $(".dz-image-preview[data-media-id='" + value.MediaFileId + "']");
								preview.attr("data-display-order", value.DisplayOrder);
								preview.attr("data-entity-media-id", value.EntityMediaId);
							});
						}
					});
				}
			}

			function setPreviewIcon(file, small) {
				var el = $(file.previewTemplate);
				var elIcon = el.find('.file-icon');
				var elImage = el.find('.file-figure > img').addClass("hide");
				var icon = SmartStore.media.getIconHint(file);

				elIcon.attr("class", "file-icon show " + icon.name + (small ? " fa-2x" : " fa-4x")).css("color", icon.color);

				if (small)
					return;

				elImage.one('load', function () {
					elImage.removeClass('hide');
					elIcon.removeClass('show');
				});
			}

			fuContainer.on("mediaselected", function (e, files) {
				if (opts.maxFiles === 1) {
					displaySingleFilePreview(files[0], fuContainer, options);

					if (options.onMediaSelected) options.onMediaSelected.apply(this, [files[0]]);
				}
				else {
					var ids = "";

					files.forEach(function (file) {
						ids += file.id + ",";
						file.media = file;
					});

					assignFilesToEntity(files, ids);
				}
			});

			// Deleting.
			$(fuContainer).on("click", ".delete-entity-picture", function (e) {

				var previewThumb = $(this).closest('.dz-image-preview');
				var entityMediaFileId = previewThumb.data("entity-media-id");
				var mediaFileId = previewThumb.data("media-id");

				$.ajax({
					async: false,
					cache: false,
					type: 'POST',
					url: $el.data('remove-url'),
					data: { id: entityMediaFileId },
					success: function () {
						previewThumb.tooltip("hide");
						previewThumb.remove();

						// File must be removed from dropzone if it was added in current queue.
						var file = el.files.find(file => file.media.id === mediaFileId);
						if (file)
							el.removeFile(file);
					}
				});

				return false;
			});

			// Show summary.
			$(fuContainer).on("click", ".open-upload-summmary", function (e) {
				elStatus.show();
				return false;
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

			// Cancel all uploads.
			elCancel.on('click', function (e) {
				e.preventDefault();
				cancelAllUploads(false);
				return false;
			});

			elStatusWindow.on('uploadcanceled', function (e) {
				cancelAllUploads(true);
			});

			elStatusWindow.on('uploadresumed', function (e) {
				var dupeFiles = el.getFilesWithStatus(Dropzone.ERROR)
					.filter(file => file.media && file.media.dupe === true);

				// TODO: DRY > make function and pass dupeFiles as param
				if (dupeFiles.length !== 0 && !dialog.isOpen) {
					dialog.open({
						queue: SmartStore.Admin.Media.convertDropzoneFileQueue(dupeFiles),
						callerId: elDropzone.find(".fileupload").attr("id"),
						onResolve: dupeFileHandlerCallback,
						onComplete: dupeFileHandlerCompletedCallback
					});
				}
			});

			function cancelAllUploads(removeFiles) {

				var currentlyUploading = el.getFilesWithStatus(Dropzone.QUEUED);
				
				// Add currently uploading file to queued files.
				currentlyUploading.push(el.getFilesWithStatus(Dropzone.UPLOADING)[0]);

				// Status
				if (elStatusWindow.length > 0) {
					elStatusWindow.find(".current-file-count").text(currentlyUploading.length);
					elStatusWindow.find(".current-file-text").text(Res['FileUploader.StatusWindow.Canceled.File' + (currentlyUploading.length === 1 ? "" : "s")]);
					elStatusWindow.find(".flyout-commands").removeClass("show");
					elStatusWindow.data("data-upload-in-progress", false);
				}
				else {
					$(this).hide();
				}

				for (file of currentlyUploading) {
					if (!file)
						return;

					if (removeFiles) {
						el.removeFile(file);
						//el.cancelUpload(file);
					}
					else {
						file.status = Dropzone.CANCELED;
						var template = $(file.previewTemplate);
						template.addClass("canceled");

						/*
						var icon = template.find(".upload-status > i");
						icon.removeClass("d-none").addClass("fa-times-circle text-danger");
						*/

						var icon = template.find(".upload-status > .fu-item-canceled");
						icon.removeClass("d-none");

						template.find(".circular-progress").remove();
					}
				}

				if (options.onAborted)
					options.onAborted.apply(this, [e, el]);

				el.emit("queuecomplete");
			}

			// On preview container close (StatusWindow)
			$(document).on("click", ".fu-status-window .close-status-window", function () {
				// Only reset dropzone if there are no more files uploading. Else uploads will be canceled by uploadcanceled event.
				if (el.getFilesWithStatus(Dropzone.UPLOADING).length === 0) {
					el.removeAllFiles();
				}
			});
		});
	};

	// Global events
	var fuContainer = $('.fileupload-container');

	// Highlight dropzone element when a file is dragged into it.
	fuContainer.on("dragover", function (e) {
		var el = $(this);
		if (el.hasClass("dz-highlight"))
			return;

		el.addClass("dz-highlight");

	}).on("dragleave", function (e) {
		if ($(e.relatedTarget).closest('.fileupload-container').length === 0) {
			var el = $(this);
			if (!el.hasClass("dz-highlight"))
				return;

			el.removeClass("dz-highlight");
		}
	}).on("drop", function (e) {
		var el = $(this);
		if (!el.hasClass("dz-highlight"))
			return;

		el.removeClass("dz-highlight");
	});

	// Disable tooltips on preview sorting.
	$(document).on("dragstart", ".dz-image-preview", function (e) {
		$(".dz-image-preview").tooltip("disable");
	}).on("dragend", ".dz-image-preview", function (e) {
		$(".dz-image-preview").tooltip("enable");
	});

	// Callback function for duplicate file handling dialog.
	function dupeFileHandlerCallback(resolutionType, remainingFiles) {
		var fuContainer = $("#" + this.callerId).closest(".fileupload-container");
		var dropzone = Dropzone.forElement(fuContainer[0]);
		var errorFiles = dropzone.getFilesWithStatus(Dropzone.ERROR);
		var displayPreviewInList = fuContainer.find(".preview-container").data("display-list-items");
		var resumeUpload = false;
		var applyToRemaining = remainingFiles.length > 1;
		// Store user decision where it can be accessed by other events (e.g. dropzone > sending).
		fuContainer.data("resolution-type", resolutionType);

		// Get all duplicate files.
		var dupeFiles = errorFiles.filter(file => file.media && file.media.dupe === true);

		if (!applyToRemaining) {
			var firstFile = dupeFiles[0];
			firstFile.resolutionType = resolutionType;

			// Do nothing on skip.
			if (resolutionType === "0") {
				dropzone.removeFile(firstFile);

				if (dupeFiles[1]) {
					dialog.next();
				}
				else {
					dropzone.emit("queuecomplete");
					dialog.close();
				}

				return;
			}

			// Reset file status.
			resetFileStatus(firstFile);

			// Process first file. 
			dropzone.processFile(firstFile);
			resumeUpload = displayPreviewInList;

			// If current file is last file > close dialog else display next file.
			if (dupeFiles.length === 1) {
				dialog.close();
			}
			else {
				dialog.next();
			}
		}
		else {
			// Reset file status.
			for (file of dupeFiles) {
				resetFileStatus(file);
				file.resolutionType = resolutionType;
			}

			// Do nothing on skip.
			if (resolutionType === "0") {
				dropzone.emit("queuecomplete");
				dialog.close();
				return;
			}

			if (!displayPreviewInList) {
				// Process all files and leave.
				dropzone.processFiles(dupeFiles);
			}
			else {
				// Nicer display (queue like) if files are added singulary.
				for (file of dupeFiles) {
					dropzone.processFile(file);
				}

				resumeUpload = true;
			}

			dialog.close();
		}

		if (resumeUpload) {
			// Files are being uplodad again. So display cancel bar again.
			$(".fu-status-window")
				.attr("data-upload-in-progress", true)
				.find(".flyout-commands")
				.addClass("show");
		}

		return;
	}

	function dupeFileHandlerCompletedCallback(isCanceled) {
		if (isCanceled) {
			// All pending files must be removed from dropzone.
			var fuContainer = $("#" + this.callerId).closest(".fileupload-container");
			var dropzone = Dropzone.forElement(fuContainer[0]);
			dropzone.removeAllFiles();
		}
	}

	function displaySingleFilePreview(file, fuContainer, options) {
		//fuContainer.find('.fileupload-filename').html(file.name);
		//fuContainer.find('.fileupload-filesize').html(this.filesize(file.size));
		fuContainer.find('.fileupload-thumb').css('background-image', 'url("' + file.thumbUrl + '")');

		var id = file.downloadId ? file.downloadId : file.id;
		// TODO: .find('.hidden') doesn't seems safe. Do it better.
		fuContainer.find('.hidden').val(id).trigger('change');

		if (options.showRemoveButtonAfterUpload)
			fuContainer.find('.remove').show();
	}

	function preCheckForDuplicates(addFileName, previewContainer) {
		var files = previewContainer.find(".dz-image-preview");

		var dupe = files.filter(function () {
			var mediaName = $(this).data("media-name");

			if (mediaName)
				mediaName = mediaName.toLowerCase();

			return mediaName === addFileName.toLowerCase();
		});

		return dupe.length === 1;
	}

	function updateUploadStatus(dropzone, elStatus) {
		var summary = elStatus.find(".fileupload-status-summary"),
			uploadedFileCount = summary.find(".uploaded-file-count"),
			totalFileCount = summary.find(".total-file-count"),
			errors = elStatus.find(".erroneous-files");

		var successFiles = dropzone.getFilesWithStatus(Dropzone.SUCCESS);
		var errorFiles = dropzone.getFilesWithStatus(Dropzone.ERROR);
		var otherErrors = errorFiles.filter(file => !file.media && file.message);

		// Summary.
		uploadedFileCount.text(successFiles.length);
		totalFileCount.text(dropzone.files.length);

		// Errors.
		if (otherErrors.length > 0) {
			var errorMarkUp = "";
			for (file of errorFiles) {
				errorMarkUp += "<div><span>" + file.name + "</span>";
				errorMarkUp += "<span>" + file.message + "</span></div>";
			}

			errors.find(".file-list")
				.html(errorMarkUp)
				.removeClass("d-none");
		}

		// Renamed, replaced, skipped.
		var skippedFiles = dropzone.files.filter(file => file.resolutionType === "0");
		var replacedFiles = dropzone.files.filter(file => file.resolutionType === "1");
		var renamedFiles = dropzone.files.filter(file => file.resolutionType === "2");

		fillStatusList(skippedFiles, elStatus.find(".skipped-files"));
		fillStatusList(renamedFiles, elStatus.find(".renamed-files"));
		fillStatusList(replacedFiles, elStatus.find(".replaced-files"));
	}

	function fillStatusList(files, elList) {
		if (files.length > 0) {
			var markUp = "";

			for (file of files) {
				markUp += "<div><span>" + file.name + "</span></div>";
			}

			elList.find(".file-list").html(markUp);
			elList.removeClass("d-none");
		}
	}

	function resetFileStatus(file) {
		if (file.status === Dropzone.SUCCESS) {
			file.status = undefined;
			file.accepted = undefined;
			file.processing = false;
			file.media = null;
		}

		// Reset sidebar item status here.
		var elStatusWindow = $(".fu-status-window");	// Status window is unique thus no need to pass it as a parameter.
		if (elStatusWindow.length > 0) {
			var el = $(file.previewElement);
			var icon = el.find(".upload-status > i");
			icon.addClass("d-none");
			window.setCircularProgressValue(el, 0);
			el.find(".circular-progress").removeClass("d-none");

			//dzResetProgressBar(el.find(".progress-bar"));
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

	function logEvent() {
		var keyValues = getQueryStrings();

		// Event logging can be turned on by a GET parameter e.g. ?logEvents=all || ?logEvents=eventname
		var paramValue = keyValues.logevents;
		if (paramValue === "all" || paramValue === arguments[0]) {
			console.log.apply(console, arguments);
		}
	}

})(jQuery);

