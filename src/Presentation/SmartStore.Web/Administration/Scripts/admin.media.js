SmartStore.Admin.Media = (function () {

    class FileConflictResolutionDialog {
        constructor() {
            // Private variables.
            this._url = $('head > meta[property="sm:root"]').attr('content') + 'admin/media/fileconflictresolutiondialog';
            this._dialog = null;
            this._dupeFileDisplay = null;
        }

        // Public properties
        get currentConflict() {
            return this.queue ? this.queue[this.currentIndex] : null;
        }

        get isOpen() {
            return $(this._dialog).hasClass("show");
        }

        get resolutionType() {
            let dialog = this._dialog;
            if (!dialog)
                return undefined;

            return parseInt(dialog.find('input[name=resolution-type]:checked').val());
        }

        // Public methods
        open(opts) {
            if (this.isOpen)
                return;

            const self = this;

            // Public variables.
            this.currentIndex = 0;
            this.callerId = opts.callerId;
            this.isSingleFileUpload = opts.isSingleFileUpload;
            this.queue = opts.queue;
            this.onResolve = opts.onResolve || _.noop; // return params => self, dupeFileHandlingType, saveSelection, files
            this.onComplete = opts.onComplete || _.noop; // return params => self, isCanceled
            this.closeOnCompleted = toBool(opts.closeOnCompleted, true);

            if (this.queue && this.queue.length) {
                this._ensureDialog(function () {
                    this.modal('show');
                    self.refresh();
                });
            }
        }

        close() {
            if (this._dialog && this._dialog.length) {
                this._dialog.modal('hide');
            }
        }

        next() {
            if (!this.isOpen)
                return;

            this.currentIndex++;

            var conflict = this.currentConflict;
            if (conflict) {
                this.refresh(conflict);
            }
            else {
                // End of queue is reached.
                if (this.closeOnCompleted) {
                    this.close();
                }
                else {
                    if (_.isFunction(this.onComplete))
                        this.onComplete.apply(this, [false]);
                }
            }
        }

        refresh(conflict) {
            conflict = conflict || this.currentConflict;
            if (!conflict)
                return;

            let dialog = this._dialog;
            let dupeFileDisplay = this._dupeFileDisplay;

            const existingFileDisplay = dialog.find(".existing-file-display");
            const source = conflict.source;
            const dest = conflict.dest;

            // Enable apply button.
            dialog.find(".btn-apply").removeClass("disabled");

            // Display current filename in intro text.
            dialog.find(".intro .current-file").html('<b class="fwm">' + source.name + '</b>');

            // Display remaining file count.
            dialog.find(".remaining-file-counter .current-count").text(this.currentIndex + 1);
            dialog.find(".remaining-file-counter .total-count").text(this.queue.length);

            this._refreshFileDisplay(dupeFileDisplay, source);
            this._refreshFileDisplay(existingFileDisplay, dest);

            // Trigger change to display changed filename immediately.
            $("input[name=resolution-type]:checked").trigger("change");
        }

        // Private methods
        _refreshFileDisplay(el, file) {
            var preview = SmartStore.media.getPreview(file, { iconCssClasses: "fa-4x" });
            el.find(".file-preview").html(preview.thumbHtml);
            SmartStore.media.lazyLoadThumbnails(el);

            el.find(".file-name").text(file.name);
            el.find(".file-name").attr("title", file.name);
            //el.find(".file-date").text(moment(file.createdOn).format('L LTS'));
            el.find(".file-size").text(_.formatFileSize(file.size));

            if (file.dimensions) {
                var width = parseInt(file.dimensions.split(",")[0]);
                var height = parseInt(file.dimensions.split(",")[1]);

                if (width && height) {
                    el.find(".file-dimensions").text(width + " x " + height);
                }
            }
        }

        _ensureDialog(onReady) {
            const self = this;

            if (!this._dialog || !this._dialog.length) {
                this._dialog = $("#duplicate-window");
            }

            if (this._dialog.length) {
                this._dupeFileDisplay = this._dialog.find(".dupe-file-display");
                onReady.apply(this._dialog);
                return;
            }

            // Get dialog via ajax and append to body.
            $.ajax({
                async: true,
                cache: false,
                type: 'POST',
                url: this._url,
                success(response) {
                    $("body").append($(response));
                    self._dialog = $("#duplicate-window");
                    self._dupeFileDisplay = self._dialog.find(".dupe-file-display");

                    if (self.isSingleFileUpload) {
                        self._dialog.find("#apply-to-remaining").parent().hide();
                        self._dialog.find(".remaining-file-counter").hide();
                    }

                    // Listen to change events of radio group (dupe handling type) and display name of renamed file accordingly.
                    $(self._dialog).on("change", 'input[name=resolution-type]', (e) => {
                        var fileName = self.currentConflict.dest.name;

                        if ($(e.target).val() === "2") {
                            var uniquePath = self.currentConflict.dest.uniquePath;
                            fileName = uniquePath.substr(uniquePath.lastIndexOf("/") + 1);
                        }

                        self._dupeFileDisplay
                            .find(".file-name")
                            .attr("title", fileName)
                            .text(fileName);
                    });

                    $(self._dialog).on("click", ".btn-apply", () => {
                        self._dialog.data('canceled', false);
                        var applyToRemaining = self._dialog.find('#apply-to-remaining').is(":checked");

                        // Display apply button until current item is processed & next item is called by refresh (prevents double clicks while the server is still busy).
                        $(this).addClass("disabled");

                        if (_.isFunction(self.onResolve)) {
                            var start = self.currentIndex;
                            var end = applyToRemaining ? self.queue.length : self.currentIndex + 1;
                            var slice = self.queue.slice(start, end);
                            if (applyToRemaining) {
                                self.currentIndex = self.queue.length - 1;
                            }

                            // Set file status for later access.
                            for (var i in slice) {
                                slice[i].resolutionType = self.resolutionType;
                            }

                            self.onResolve.apply(self, [self.resolutionType, slice]);
                        }
                        return false;
                    });

                    $(self._dialog).on("click", ".btn-cancel", () => {
                        self._dialog.data('canceled', true);
                        self.queue = null;
                        self.close();
                        return false;
                    });

                    $(self._dialog).on("hidden.bs.modal", () => {
                        if (_.isFunction(self.onComplete)) {
                            self.onComplete.apply(self, [self._dialog.data('canceled')]);
                        }

                        self._dialog.trigger("resolution-complete");

                        self.currentIndex = 0;
                        self.callerId = null;
                        self.queue = null;
                        self.onResolve = _.noop;
                        self.onComplete = _.noop;
                    });

                    onReady.apply(self._dialog);
                }
            });
        }
    }

    return {
        convertDropzoneFileQueue: function (queue) {
            return _.map(queue, function (dzfile) {
                var idx = dzfile.name.lastIndexOf('.');
                var title = idx > -1 ? dzfile.name.substring(0, idx) : dzfile.name;
                var ext = idx > -1 ? dzfile.name.substring(idx) : '';

                // Temp stub for resolving media type only
                var stub = { ext: ext, mime: dzfile.type };
                var mediaType = SmartStore.media.getIconHint(stub).mediaType;

                var file = {
                    thumbUrl: dzfile.dataURL ? dzfile.dataURL : null,
                    name: dzfile.name,
                    title: title,
                    ext: ext,
                    mime: dzfile.type,
                    type: mediaType,
                    createdOn: dzfile.lastModifiedDate,
                    size: dzfile.size,
                    width: dzfile.width ? dzfile.width : null,
                    height: dzfile.height ? dzfile.height : null,
                    dimensions: dzfile.width && dzfile.height ? dzfile.width + ", " + dzfile.height : null
                };

                return { source: file, dest: dzfile.media, original: dzfile };
            });
        },
        fileConflictResolutionDialog: new FileConflictResolutionDialog()
    };
})();