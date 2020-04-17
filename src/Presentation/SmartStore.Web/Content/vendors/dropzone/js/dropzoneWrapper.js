/*
* Dropzone Wrapper
*/

(function ($) {

	$.fn.dropzoneWrapper = function (options) {
		return this.each(function () {
			var el = $(this);
			var elDropzone = el.closest('.fileupload-container');

			// If there is no fileupload container > wrap it around. Needed to make the whole Picture Control a dropzone.
			/*
			if (elDropzone.length === 0) {
				el.wrap('<div class="fileupload-container"></div>');
				elDropzone = el.closest('.fileupload-container');
			}
			*/

			// Place multifile upload preview into the designated spot defined by Picture.
			var previewTemplate;
			if (options.maxFiles > 1 && options.previewContainerId !== "") {
				previewTemplate = $(".files-preview");
				$("#" + options.previewContainerId).append(previewTemplate);
			}

			var elRemove = elDropzone.find('.remove'),
				elCancel = elDropzone.find('.cancel'),
				elFile = elDropzone.find('.fileinput-button'),
				elProgressBar = elDropzone.find('.progress-bar');

			elDropzone.addClass("dropzone");

			// File extensions of MediaManager are dotless but dropzone expects dots.
			var acceptedFiles = "";
			if (el.data('accept')) {
				acceptedFiles = "." + el.data('accept').replace(/\,/g, ",.");
			}
			
			var opts = {
				url: el.data('upload-url'),
				clickable: el.find(".fileinput-button")[0],
				acceptedFiles: acceptedFiles,
				maxFiles: options.maxFiles,
				previewsContainer: options.previewContainerId !== "" ? "#" + options.previewContainerId : null
			};

			if (previewTemplate && previewTemplate.length !== 0) {
				opts.previewTemplate = previewTemplate[0].innerHTML;
			}

			options = $.extend({}, opts, options);

			var myDropzone = new Dropzone(elDropzone[0], options);
			var canUploadMoreFiles = true;

			myDropzone.on("addedfile", function (file) {
				
				// Reset progressbar when a new file was added.
				dzResetProgressBar(elProgressBar);
			});

			myDropzone.on("drop", function (files) {

				// Reset canUploadMoreFiles if new files have been added.
				canUploadMoreFiles = true;
			});

			myDropzone.on("maxfilesexceeded", function (file) {

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
			
			myDropzone.on("totaluploadprogress", function (progress) {
				elProgressBar
					.attr('aria-valuenow', progress)
					.css('width', progress + '%');
			});

			myDropzone.on("success", function (file, response, progress) {

				// Only for singleupload.
				if (opts.maxFiles === 1) {
					elDropzone.find('.fileupload-filename').html(file.name);
					elDropzone.find('.fileupload-filesize').html(this.filesize(file.size));
					elDropzone.find('.fileupload-thumb').css('background-image', 'url("' + response.url + '")');
					elDropzone.find('.hidden').val(response.fileId).trigger('change');
					elRemove.show();
				}

				if (options.onUploadCompleted) options.onUploadCompleted.apply(this, [file, response, progress]);
			});

			myDropzone.on("sending", function (file) {
				if (options.onUploading) options.onUploading.apply(this, [file]);
			});

			myDropzone.on("canceled", function (file) {
				if (options.onAborted) options.onAborted.apply(this, [file]);
			});

			myDropzone.on("removedfile", function (file) {

				// Reset progress bar when file was removed.
				dzResetProgressBar(elProgressBar);

				if (options.onFileRemove) options.onFileRemove.apply(this, [file]);
			});

			myDropzone.on("complete", function (file) {
				if (options.onCompleted) options.onCompleted.apply(this, [file]);
			});

			myDropzone.on("error", function (file, errMessage, xhr) {

				if (xhr && file.status === "error") {
					displayNotification(xhr.statusText, "error");
				}

				// Multifile only.
				if (!file.accepted && opts.maxFiles !== 1) {
					displayNotification(errMessage, "error");
					return;
				}
				
				if (options.onError) options.onError.apply(this, [file, errMessage]);
			});

			// Used to highlight dropzone when a file is dragged into the browser.
			$(document).bind("dragover", function (e) {
				if (elDropzone.hasClass("dz-highlight"))
					return;

				elDropzone.addClass("dz-highlight");

			}).bind("dragleave drop", function (e) {

				if (!elDropzone.hasClass("dz-highlight"))
					return;

				elDropzone.removeClass("dz-highlight");
			});

			elFile.on('click', function (e) {

				// Reset canUploadMoreFiles if a new file is about to be selected vie FileOpen dialog.
				canUploadMoreFiles = true;
			});

			// Remove uploaded file.
			elRemove.on('click', function (e) {

				elDropzone.find('.fileupload-thumb').css('background-image', 'url("' + el.data('fallback-url') + '")');
				elDropzone.find('.hidden').val(0).trigger('change');

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
		elProgressBar
			.attr('aria-valuenow', 0)
			.css('width', 0 + '%');
	}

})(jQuery);
