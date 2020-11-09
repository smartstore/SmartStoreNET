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
    var dialog = SmartStore.Admin ? SmartStore.Admin.Media.fileConflictResolutionDialog : null;
    var token = $('input[name="__RequestVerificationToken"]').val();

    $.fn.dropzoneWrapper = function (options) {
        return this.each(function () {
            var el = this, $el = $(this);

            var elDropzone = $el.closest('.dropzone-target'),
                fuContainer = $el.closest('.fu-container');

            if (!fuContainer.length) {
                $el.closest('.dropzone-container').wrap('<div class="fu-container h-100"></div>');
                fuContainer = $el.closest('.fu-container');
            }

            var elRemove = fuContainer.find('.remove'),
                elProgressBar = fuContainer.find('.progress-bar'),
                elStatusWindow = $(".fu-status-window"),
                previewContainer = fuContainer.find(".preview-container");

            var displayPreviewInList = previewContainer.data("display-list-items");

            // Init dropzone.
            elDropzone.addClass("dropzone");

            // Dropzone init params.
            var opts = {
                url: $el.data('upload-url'),
                clickable: options.clickableElement ? options.clickableElement : elDropzone.find(".fu-message")[0],
                hiddenInputContainer: fuContainer[0],
                parallelUploads: 1,
                uploadMultiple: true,
                acceptedFiles: $el.data('accept'),
                maxFiles: options.maxFiles,
                previewsContainer: options.previewContainerId !== "" ? "#" + options.previewContainerId : null
            };

            if (options.maxFilesSize > 0) {
                opts.maxFilesize = Math.round(options.maxFilesSize / 1024);
            }

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

            // SingleFile only
            if (opts.maxFiles === 1) {
                var currentFileId = parseInt(fuContainer.find('.hidden').val());

                if (!currentFileId || currentFileId === 0) {
                    // Display icon according to type filter.
                    setSingleFilePreviewIcon(fuContainer, $el.data("type-filter"));
                }
                else {
                    
                    // Load thumbnail.
                    SmartStore.media.lazyLoadThumbnails(fuContainer.find('.fu-thumb'));

                    // Set current filename as fu-message on init.
                    fuContainer.find(".fu-message").removeClass("empty").html(fuContainer.find(".fu-filename").data("current-filename"));

                    if (options.showRemoveButton)
                        elRemove.addClass("d-flex");
                }
            }

            // Init sorting  if preview items aren't displayed in a list.
            if (!displayPreviewInList && options.maxFiles > 1) {
                previewContainer.sortable({
                    items: fuContainer.find('.dz-image-preview'),
                    handle: '.drag-gripper',
                    ghostClass: 'sortable-ghost',
                    animation: 150
                }).on('sort', function (e, ui) {
                    sortMediaFiles();
                });
            }

            options = $.extend({}, opts, options);
            el = new Dropzone(fuContainer[0], options);

            // BEGIN: Dropzone events.
            el.on("addedfile", function (file) {
                logEvent("addedfile", file);

                if (opts.maxFiles !== 1)
                    setPreviewIcon(file, displayPreviewInList);

                if (displayPreviewInList) {
                    // Status window.
                    var progress = window.createCircularSpinner(36, true, 6, null, null, true, true, true);
                    $(file.previewTemplate)
                        .find(".upload-status")
                        .attr("data-uuid", file.upload.uuid)
                        .append(progress);
                } else if (opts.maxFiles !== 1) {
                    // Entity assignment preview.
                    $(file.previewTemplate).find(".fu-file-info-name").html(file.name);

                    // If file is a duplicate prevent it from being displayed in preview container.
                    if (preCheckForDuplicates(file.name, previewContainer)) {
                        $(file.previewTemplate).addClass("d-none");
                    }
                }
            });

            el.on("addedfiles", function (files) {
                logEvent("addedfiles", files);

                // Status
                if (elStatusWindow.length > 0) {
                    elStatusWindow.find(".current-file-count").text(files.length);
                    elStatusWindow.find(".current-file-text").text(Res['FileUploader.StatusWindow.Uploading.File' + (files.length === 1 ? "" : "s")]);

                    var queuedFiles = el.getFilesWithStatus(Dropzone.QUEUED);
                    if (queuedFiles.length > 1) {
                        swapFlyoutCommands(true);
                        elStatusWindow.find(".flyout-commands").addClass("show");
                        elStatusWindow.data("upload-in-progress", true);
                        elStatusWindow.data("files-in-queue", true);
                    }
                }
            });

            el.on("processing", function (file) {
                var currentProcessingCount = el.getFilesWithStatus(Dropzone.PROCESSING).length;
                logEvent("processing", file, currentProcessingCount);

                if (displayPreviewInList)
                    previewContainer.scrollTo(file.previewElement);

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

                if (token !== "" && !formData.get("__RequestVerificationToken"))
                    formData.append("__RequestVerificationToken", token);

                if (file.fullPath) {
                    var directory = file.fullPath.substring(0, file.fullPath.lastIndexOf('/'));
                    if (directory)
                        formData.append("directory", directory);
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
                if (token !== "" && !formData.get("__RequestVerificationToken"))
                    formData.append("__RequestVerificationToken", token);
                
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
                else if (displayPreviewInList) {
                    var template = $(file.previewTemplate);
                    template.removeClass("dz-image-preview");
                    var icon = template.find(".upload-status > i");
                    icon.removeClass("d-none");
                    template.find(".circular-progress").addClass("d-none");
                }

                // If there was an error returned by the server set file status accordingly.
                if (response.length) {
                    for (var fileResponse of response) {
                        if (!fileResponse.success) {
                            file.status = Dropzone.ERROR;
                            file.media = fileResponse;
                        }
                    }
                }
                else {
                    file.media = response;

                    if (!response.success) file.status = Dropzone.ERROR;
                }

                if (options.onUploadCompleted) options.onUploadCompleted.apply(this, [file, response, progress]);
            });

            el.on("successmultiple", function (files, response, progress) {
                logEvent("successmultiple", files, response, progress);

                // Handle notifications
                var xhr = progress.currentTarget;
                if (xhr && xhr.getResponseHeader) {
                    var msg = xhr.getResponseHeader('X-Message');
                    if (msg) {
                        showNotification(msg, xhr.getResponseHeader('X-Message-Type'));
                    }
                }

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

                if (files.length === 1) {
                    assignableFiles.push(files[0]);
                }
                else if (files.length) {
                    assignableFiles = assignableFiles.concat(files);
                }
                else {
                    assignableFiles.push(files);
                }
            });

            el.on("complete", function (file) {
                logEvent("complete", file);

                // Reset dropzone for single file uploads, so other files can be uploaded again.
                // (opts.maxFiles === 1 && file.media && file.media.dupe === false) > Reset for SingleFileUploads if there are no dupes.
                // !file.media	> Some upload actions might not set media because they are not uploading to MM. 
                // !file.status === Dropzone.ERROR > If file upload fails there's also no media
                // TODO: !file.media doesn't feel right. Better give control to the action by returning a corresponding value
                if ((opts.maxFiles === 1 && file.media && !file.media.dupe) || (!file.media && !file.status === Dropzone.ERROR)) {
                    this.removeAllFiles(true);
                }

                //if (options.onUploadCompleted) options.onUploadCompleted.apply(this, [file]);
            });

            el.on("completemultiple", function (files) {
                logEvent("completemultiple", files);
                logEvent("completemultiple", " > assignableFiles.length, assignableFileIds", assignableFiles.length, assignableFileIds);

                // Dupe file handling is 'replace' thus no need for assignment to entity (media IDs remain the same, while file was altered). 
                if (parseInt(fuContainer.data("resolution-type")) === 1) {
                    // Update preview pic of replaced media file.
                    for (var newFile of files) {
                        var elCurrentFile = previewContainer.find(".dz-image-preview[data-media-id='" + newFile.media.id + "']");
                        elCurrentFile.find("img").attr("src", newFile.dataURL);
                        this.removeFile(newFile);
                    }
                }
            });

            el.on("canceledmultiple", function (files) {
                logEvent("canceledmultiple", files);
            });

            el.on("queuecomplete", function () {
                logEvent("queuecomplete");
                
                var dupeFiles = this.getFilesWithStatus(Dropzone.ERROR).filter(file => file.media && file.media.dupe === true);
                var successFiles = this.getFilesWithStatus(Dropzone.SUCCESS);

                // If there are duplicates & dialog isn't already open > open duplicate file handler dialog.
                if (dupeFiles.length !== 0 && dialog && !dialog.isOpen) {

                    // Close confirmation dialog. User was too slow. Uploads are complete.
                    if (elStatusWindow.data("confirmation-requested")) {
                        $("#modal-confirm-shared").modal("hide");
                    }

                    // Open duplicate file handler dialog.
                    dialog.open({
                        queue: SmartStore.Admin.Media.convertDropzoneFileQueue(dupeFiles),
                        callerId: elDropzone.find(".fu-fileupload").attr("id"),
                        onResolve: dupeFileHandlerCallback,
                        onComplete: dupeFileHandlerCompletedCallback,
                        isSingleFileUpload: options.maxFiles === 1
                    });
                }

                if (dupeFiles.length === 0) {
                    assignFilesToEntity(assignableFiles, assignableFileIds, true);
                }
                else {
                    // Duplicate resolution may not be done yet.
                    if (!dialog.isOpen && dupeFiles.length > 0) {
                        assignableFileIds = "";
                        assignableFiles.length = 0;
                    }
                }

                // Status
                if (elStatusWindow.length > 0) {
                    elStatusWindow.find(".current-file-count").text(successFiles.length ? successFiles.length : 0);
                    elStatusWindow.find(".current-file-text").text(Res['FileUploader.StatusWindow.Complete.File' + (successFiles.length === 1 ? "" : "s")]);

                    // Only hide commands if no uploads were canceled.
                    var canceledFiles = this.getFilesWithStatus(Dropzone.CANCELED);
                    if (canceledFiles.length === 0) {
                        elStatusWindow.find(".flyout-commands").removeClass("show");
                    }

                    elStatusWindow.data("files-in-queue", canceledFiles.length !== 0);
                    elStatusWindow.data("upload-in-progress", false);
                }

                // Reset progressbar when queue is complete.
                if (opts.maxFiles === 1) {
                    // SingleFile
                    dzResetProgressBar(elProgressBar);
                }
                else if (!displayPreviewInList || (displayPreviewInList && dupeFiles.length !== 0)) { // Don't reset progress bar for status window if dupefiles = 0
                    // MultiFile
                    var uploadedFiles = this.files;

                    for (var file of uploadedFiles) {
                        // Only reset progress bar if there was an error (e.g. file is dupe) and the files must be processed again.
                        if (file.status === Dropzone.ERROR) {
                            dzResetProgressBar($(file.previewElement).find(".progress-bar"));
                        }
                    }
                }

                if (options.onCompleted) options.onCompleted.apply(this, [successFiles, dupeFiles.length === 0]);
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

                    // Can be removed when issue was resolved (dropzone Update 5.8.03)
                    // https://gitlab.com/meno/dropzone/-/issues/217
                    if (errMessage.indexOf(el.options.timeout) > 0)
                        errMessage = errMessage.replace(el.options.timeout, el.options.timeout / 1000);
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
            });

            el.on("maxfilesexceeded", function (file) {
                logEvent("maxfilesexceeded", file);
            });
            // END: Dropzone events.

            // Calls server function to assign current uploads to the entity.
            function assignFilesToEntity(assignableFiles, assignableFileIds, clearAssignableFiles) {
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
                            entityId: $el.data('entity-id'),
                            __RequestVerificationToken: token 
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
                                        .addClass("dz-success dz-complete")
                                        .removeClass("d-none dz-processing");

                                    elPreview.find(".fu-file-info-name").html(value.Name);

                                    elPreview
                                        .find('img')
                                        .attr('src', file.dataURL || file.media.thumbUrl);

                                    previewContainer.append(elPreview);
                                    dzResetProgressBar(elPreview.find(".progress-bar"));
                                }
                                else {
                                    console.log("Error while adding preview element.", value.Name.toLowerCase());
                                }
                            });

                            if (clearAssignableFiles) {
                                assignableFileIds = "";
                                assignableFiles.length = 0;
                            }
                            sortMediaFiles();
                        }
                    });
                }
            }

            // Calls server function after sorting, to save current sort order.
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
                            entityId: $el.data('entity-id'),
                            __RequestVerificationToken: token 
                        },
                        success: function (response) {
                            // Set EntityMediaId & current DisplayOrder.
                            $.each(response.response, function (index, value) {
                                var preview = $(".dz-image-preview[data-media-id='" + value.MediaFileId + "']");
                                preview.attr("data-display-order", value.DisplayOrder);
                                preview.attr("data-entity-media-id", value.EntityMediaId);

                                if (index === 0) {
                                    // Update preview pic in upper left corner of product detail page.
                                    var productThumb = $(".section-header .title > img");
                                    productThumb.attr("src", preview.find("img").attr("src"));
                                }
                            });
                        }
                    });
                }
            }

            // Sets file icon for files in preview containers (in multifile & status window).
            function setPreviewIcon(file, small) {
                var el = $(file.previewTemplate);
                var elIcon = el.find('.file-icon');
                var elImage = el.find('.file-figure > img').addClass("hide");

                // Convert dz file property to sm file property if not already set.
                if (!file.mime)
                    file.mime = file.type;

                var icon = SmartStore.media.getIconHint(file);

                elIcon.attr("class", "file-icon show " + icon.name + (small ? " fa-2x" : " fa-4x")).css("color", icon.color);

                if (small)
                    return;

                elImage.one('load', function () {
                    elImage.removeClass('hide');
                    elIcon.removeClass('show');
                });
            }

            // Is called after files were selected by the MediaManager plugin.
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

            // Deletes a media file assignment of an entity (multifile upload).
            fuContainer.on("click", ".delete-entity-picture", function (e) {
                e.preventDefault();
                var removeUrl = $el.data('remove-url');
                if (removeUrl) {
                    var previewThumb = $(this).closest('.dz-image-preview');
                    var entityMediaFileId = previewThumb.data("entity-media-id");
                    var mediaFileId = previewThumb.data("media-id");

                    $.ajax({
                        cache: false,
                        type: 'POST',
                        url: removeUrl,
                        data: {
                            id: entityMediaFileId,
                            __RequestVerificationToken: token 
                        },
                        success: function () {
                            previewThumb.remove();

                            // File must be removed from dropzone if it was added in current queue.
                            var file = el.files.find(file => file.media.id === mediaFileId);
                            if (file) {
                                el.removeFile(file);
                            }
                        }
                    });
                }
            });

            // Remove uploaded file (singlefile upload).
            elRemove.on('click', function (e) {
                e.preventDefault();
                setSingleFilePreviewIcon(fuContainer, $el.attr("data-type-filter"));
                fuContainer.find('.fu-message').html(Res['FileUploader.Dropzone.Message']);
                fuContainer.find('.hidden').val(0).trigger('change');
                $(this).removeClass("d-flex");

                if (options.onFileRemove)
                    options.onFileRemove.apply(this, [e, el]);

                return false;
            });

            // BEGIN: Status window in MediaManager plugin.
            elStatusWindow.on('click', '.flyout-commands .resume', function (e) {
                resumeAllUploads();
                swapFlyoutCommands(true);
                return false;
            });

            elStatusWindow.on('click', '.fu-item-canceled', function (e) {
                var uuid = $(this).closest(".upload-status").attr("data-uuid");
                var file = el.files.filter(file => file.upload.uuid === uuid)[0];
                resetFileStatus(file);
                el.processFile(file);
                tryCLoseFlyoutCommands();
                return false;
            });

            elStatusWindow.on('uploadcanceled', function (e, removeFiles) {
                cancelAllUploads(removeFiles);
            });

            function resumeAllUploads() {
                var canceledFiles = el.getFilesWithStatus(Dropzone.CANCELED);

                for (var file of canceledFiles) {
                    resetFileStatus(file);
                    el.processFile(file);
                }

                tryCLoseFlyoutCommands();
            }

            function tryCLoseFlyoutCommands() {
                // If only one upload is in progress hide commands.
                var currentlyUploading = el.getFilesWithStatus(Dropzone.QUEUED);

                if (currentlyUploading.length === 0) {
                    elStatusWindow.find(".flyout-commands").removeClass("show");
                    elStatusWindow.data("upload-in-progress", false);
                }
            }

            function swapFlyoutCommands(showCancel) {
                var cancel = elStatusWindow.find(".flyout-commands .cancel");
                var resume = elStatusWindow.find(".flyout-commands .resume");

                if (showCancel) {
                    cancel.removeClass("d-none");
                    resume.addClass("d-none");
                }
                else {
                    cancel.addClass("d-none");
                    resume.removeClass("d-none");
                }
            }

            function cancelAllUploads(removeFiles) {
                var currentlyUploading = el.getFilesWithStatus(Dropzone.QUEUED);

                // Status
                if (elStatusWindow.length > 0) {
                    elStatusWindow.find(".current-file-count").text(currentlyUploading.length);
                    elStatusWindow.find(".current-file-text").text(Res['FileUploader.StatusWindow.Canceled.File' + (currentlyUploading.length === 1 ? "" : "s")]);
                    swapFlyoutCommands(false);
                    elStatusWindow.data("upload-in-progress", false);
                }
                else {
                    $(this).hide();
                }

                if (removeFiles) {
                    for (var s of el.getFilesWithStatus(Dropzone.SUCCESS)) {
                        el.removeFile(s);
                    }

                    elStatusWindow.data("upload-in-progress", currentlyUploading.length !== 0);
                }
                else {
                    for (var file of currentlyUploading) {
                        if (!file)
                            return;

                        if (file.status !== Dropzone.UPLOADING) {
                            file.status = Dropzone.CANCELED;
                            var template = $(file.previewTemplate).addClass("canceled");
                            template.find(".upload-status > .fu-item-canceled").removeClass("d-none");
                            template.find(".circular-progress").addClass("d-none");
                        }
                    }
                }

                if (options.onAborted)
                    options.onAborted.apply(this, [e, el]);

                el.emit("queuecomplete");
            }
            // END: Status window in MediaManager plugin.

            $(document).one("resolution-complete", "#duplicate-window", function () {
                // Singlefileupload needs no resolution complete handling.
                if (opts.maxFiles === 1)
                    return;

                if (options.onCompleted && dialog.queue) {
                    var files = {};
                    for (var file of dialog.queue) {
                        files[file.dest.id] = file.dest;
                    }

                    options.onCompleted.apply(this, [files, true]);
                }

                // Status window won't assign any entities.
                if (displayPreviewInList)
                    return;

                if (dialog.queue) {
                    var fileIds = "";
                    var filesToAssign = [];

                    for (var f of el.files) {
                        if (assignableFileIds.indexOf(f.media.id) === -1) {
                            fileIds += f.media.id + ",";
                            filesToAssign.push(f);
                        }
                    }

                    var skippedFiles = dialog.queue.filter(x => x.resolutionType === 3);
                    if (skippedFiles.length) {

                        for (var skipped of skippedFiles) {
                            fileIds += skipped.dest.id + ",";
                            filesToAssign.push(skipped.original);
                        }
                    }

                    if (fileIds !== "")
                        assignFilesToEntity(filesToAssign, fileIds, false);
                }
            });
        });
    };

    // Global events
    var fuContainer = $('.fu-container');

    // Highlights dropzone element when a file is dragged into it.
    fuContainer.on("dragover", function (e) {
        var el = $(this);
        if (el.hasClass("dz-highlight"))
            return;

        el.addClass("dz-highlight");

    }).on("dragleave", function (e) {
        if ($(e.relatedTarget).closest('.fu-container').length === 0) {
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

    // Callback function for duplicate file resolution dialog. Will be called after each click on apply.
    // remainingFiles: either all files (if apply to remaining was checked) or the one file of the current resolution.
    function dupeFileHandlerCallback(resolutionType, remainingFiles) {
        var fuContainer = $("#" + this.callerId).closest(".fu-container");
        var dropzone = Dropzone.forElement(fuContainer[0]);
        var errorFiles = dropzone.getFilesWithStatus(Dropzone.ERROR);
        var displayPreviewInList = fuContainer.find(".preview-container").data("display-list-items");
        var resumeUpload = false;
        var applyToRemaining = remainingFiles.length > 1;
        // Store user decision where it can be accessed by other events (e.g. dropzone > sending).
        fuContainer.data("resolution-type", resolutionType);

        var dupeFiles = errorFiles.filter(file => file.media && file.media.dupe === true);

        if (!applyToRemaining) {
            var firstFile = dupeFiles[0];

            // Do nothing on skip.
            if (resolutionType === 3) {
                firstFile.media.dupe = false;

                if (dupeFiles[1]) {
                    dialog.next();
                }
                else {
                    dropzone.emit("queuecomplete");
                    dialog.close();
                }
                fuContainer.data("resolution-type", "");
                return;
            }

            resetFileStatus(firstFile);
            dropzone.processFile(firstFile);
            resumeUpload = displayPreviewInList;

            // If current file is last file: close dialog else display next file.
            if (dupeFiles.length === 1) {
                dialog.close();
            }
            else {
                dialog.next();
            }
        }
        else {
            // Reset file status.
            for (var file of dupeFiles) {
                resetFileStatus(file);
            }

            // Do nothing on skip.
            if (resolutionType === 3) {
                fuContainer.data("resolution-type", "");
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
                for (var dupe of dupeFiles) {
                    dropzone.processFile(dupe);
                }

                resumeUpload = true;
            }

            dialog.close();
        }

        var queuedFiles = dropzone.getFilesWithStatus(Dropzone.QUEUED);

        if (resumeUpload && queuedFiles.length > 0) {
            // Files are being uplodad again. So display cancel bar again.
            var elStatusWindow = $(".fu-status-window");
            elStatusWindow
                .data("upload-in-progress", true)
                .find(".flyout-commands")
                .addClass("show");

            var canceledFiles = dropzone.getFilesWithStatus(Dropzone.CANCELED);

            if (canceledFiles.length) {
                elStatusWindow.find(".flyout-commands .resume").removeClass("d-none");
                elStatusWindow.find(".flyout-commands .cancel").addClass("d-none");
            }
            else {
                elStatusWindow.find(".flyout-commands .resume").addClass("d-none");
                elStatusWindow.find(".flyout-commands .cancel").removeClass("d-none");
            }
        }

        // Reset resolution type.
        fuContainer.data("resolution-type", "");
        return;
    }

    // Callback function for duplicate file resolution dialog. Will be called after duplicate queue was completely resolved.
    function dupeFileHandlerCompletedCallback(isCanceled) {
        var fuContainer = $("#" + this.callerId).closest(".fu-container");
        var dropzone = Dropzone.forElement(fuContainer[0]);

        if (isCanceled) {
            // All pending files must be removed from dropzone.
            var errorFiles = dropzone.getFilesWithStatus(Dropzone.ERROR);

            for (var file of errorFiles) {
                dropzone.removeFile(file);
            }
        }

        if (dropzone.options.maxFiles === 1) {
            // Reset dropzone for single file uploads, so other files can be uploaded again.
            dropzone.removeAllFiles(true);
        }
    }

    // Sets the preview image of a single file upload control after upload or selection (by MM plugin).
    function displaySingleFilePreview(file, fuContainer, options) {
        var preview = SmartStore.media.getPreview(file, { iconCssClasses: "fa-4x" });
        fuContainer.find('.fu-thumb').removeClass("empty").html(preview.thumbHtml);
        SmartStore.media.lazyLoadThumbnails(fuContainer.find('.fu-thumb'));

        var id = file.downloadId ? file.downloadId : file.id;

        if (!options.downloadEnabled) {
            fuContainer.find('.hidden').val(id).trigger('change');
        }

        fuContainer.find('.fu-message').removeClass("empty").html(file.name);

        if (options.downloadEnabled) {
            fuContainer.on("click", '.dz-hidden-input', function (e) {
                if (!fuContainer.find('.fu-message').hasClass("empty"))
                    e.preventDefault();
            });
        }

        if (options.showRemoveButtonAfterUpload)
            fuContainer.find('.remove').addClass("d-flex");
    }

    // Sets the file icon for an empty single file upload control.
    function setSingleFilePreviewIcon(fuContainer, typeFilter) {
        var types = typeFilter.split(",");
        var icon;

        for (var type of types) {
            type = type.trim();
            var o = {};
            o[type[0] === '.' ? 'ext' : 'type'] = type;
            icon = SmartStore.media.getIconHint(o);
            if (!icon.isFallback) {
                break;
            }
        }

        icon = icon || SmartStore.media.getIconHint({});

        var html = '<i class="file-icon show fa-2x ' + icon.name + '"></i>';
        fuContainer.find('.fu-thumb').addClass("empty").html(html);
        fuContainer.find('.fu-message').addClass("empty");
    }

    // Checks for duplicates before uploading so the UI can sort them out before being displayed in preview container.
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

    // Resets file status of dz-file to initial state & circular progress of a file in status window in MM plugin.
    function resetFileStatus(file) {
        if (file.status === Dropzone.SUCCESS) {
            file.status = undefined;
            file.accepted = undefined;
            file.processing = false;
            file.media = null;
        }

        // Reset status window item status here.
        // Status window is unique thus no need to pass it as a parameter.
        var elStatusWindow = $(".fu-status-window");
        if (elStatusWindow.length > 0) {
            var el = $(file.previewElement);
            window.setCircularProgressValue(el, 0);
            el.find(".upload-status > i").addClass("d-none");
            el.find(".fu-item-canceled").addClass("d-none");
            el.find(".circular-progress").removeClass("d-none");
            el.removeClass("dz-processing dz-complete canceled");
        }
    }

    // Resets the progress bar. Don't confuse it with the circular progress in status window in MM plugin.
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

    // Logs any dropzone event. Can be invoked by get parameter where value is the event you want to be logged (e.g. logEvents=error || logEvents=success, to log every event use logEvents=all)
    function logEvent() {
        var keyValues = getQueryStrings();

        // Event logging can be turned on by a GET parameter e.g. ?logEvents=all || ?logEvents=eventname
        var paramValue = keyValues.logevents;
        if (paramValue === "all" || paramValue === arguments[0]) {
            console.log.apply(console, arguments);
        }
    }

    var showNotification = _.throttle(function (msg, type) {
        displayNotification(decodeURIComponent(escape(msg)), type);
    }, 750, { leading: true, trailing: true });

})(jQuery);

