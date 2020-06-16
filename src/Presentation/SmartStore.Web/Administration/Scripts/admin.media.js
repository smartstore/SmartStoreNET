SmartStore.Admin.Media = (function () {
	function FileConflictResolutionDialog() {
		var self = this;

		// Private variables.
		var _url = $('head > meta[property="sm:root"]').attr('content') + 'admin/media/fileconflictresolutiondialog';
		var _dialog = null;
		var _dupeFileDisplay = null;

		Object.defineProperty(this, 'currentConflict', {
			get: function () {
				return this.queue ? this.queue[this.currentIndex] : null;
			}
		});

		Object.defineProperty(this, 'isOpen', {
			get: function () { return $(_dialog).hasClass("show"); }
		});

		Object.defineProperty(this, 'resolutionType', {
			get: function () {
				if (!_dialog)
					return undefined;

				return parseInt(_dialog.find('input[name=resolution-type]:checked').val());
			}
		});

		// Public functions.
		this.open = function (opts) {
			if (this.isOpen)
				return;

			// Public variables.
			this.currentIndex = 0;
			this.callerId = opts.callerId;
			this.queue = opts.queue;
			this.onResolve = opts.onResolve || _.noop; // return params => self, dupeFileHandlingType, saveSelection, files
			this.onComplete = opts.onComplete || _.noop; // return params => self, isCanceled
			this.closeOnCompleted = toBool(opts.closeOnCompleted, true);

			if (this.queue && this.queue.length) {
				ensureDialog(function () {
					this.modal('show');
					self.refresh();
				});
			}
		};

		this.close = function () {
			if (_dialog && _dialog.length) {
				_dialog.modal('hide');
			}
		};

		this.next = function () {
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
		};

		// Private functions.
		this.refresh = function (conflict) {
			conflict = conflict || this.currentConflict;
			if (!conflict)
				return;

			var existingFileDisplay = _dialog.find(".existing-file-display");
			var source = conflict.source;
			var dest = conflict.dest;

			// Display current filename in intro text.
			_dialog.find(".intro .current-file").html('<b class="font-weight-medium">' + source.name + '</b>');

			// Display uploaded file.
			var elIcon = _dupeFileDisplay.find(".file-icon");
			var elImage = _dupeFileDisplay.find(".file-img");

			if (!source.thumbUrl) {
				// Dropzone couldn't fetch a preview for the file currently uploading.
				var icon = SmartStore.media.getIconHint(dest);
				elIcon.attr("class", "file-icon fa-4x " + icon.name).css("color", icon.color);
				elImage.addClass("d-none");
			}
			else {
				elIcon.attr("class", "file-icon");
				elImage.attr("src", source.thumbUrl).removeClass("d-none");
			}

			refreshFileDisplay(_dupeFileDisplay, source);

			// Display existing file.
			existingFileDisplay.find(".file-img").attr("src", dest.thumbUrl);
			refreshFileDisplay(existingFileDisplay, dest);
		};

		var refreshFileDisplay = function (el, file) {
			el.find(".file-name").text(file.name);
			//el.find(".file-date").text(moment(file.createdOn).format('L LTS'));
			el.find(".file-size").text(_.formatFileSize(file.size));

			if (file.dimensions) {
				var width = parseInt(file.dimensions.split(",")[0]);
				var height = parseInt(file.dimensions.split(",")[1]);

				if (width && height) {
					el.find(".file-dimensions").text(width + " x " + height);
				}
			}
		};

		function ensureDialog(onReady) {
			if (!_dialog || !_dialog.length) {
				_dialog = $("#duplicate-window");
			}

			if (_dialog.length) {
				_dupeFileDisplay = _dialog.find(".dupe-file-display");
				onReady.apply(_dialog);
				return;
			}

			// Get dialog via ajax and append to body.
			$.ajax({
				async: true,
				cache: false,
				type: 'POST',
				url: _url,
				success: function (response) {
					$("body").append($(response));
					_dialog = $("#duplicate-window");
					_dupeFileDisplay = _dialog.find(".dupe-file-display");

					// Listen to change events of radio group (dupe handling type) and display name of renamed file accordingly.
					$(_dialog).on("change", 'input[name=resolution-type]', function (e) {
						var fileName = self.currentConflict.dest.name;

						if ($(e.target).val() === "2") {
							var uniquePath = self.currentConflict.dest.uniquePath;
							fileName = uniquePath.substr(uniquePath.lastIndexOf("/") + 1);
						}

						_dupeFileDisplay.find(".file-name").text(fileName);
					});

					$(_dialog).on("click", ".btn-apply", function () {
						_dialog.data('cancelled', false);
						var applyToRemaining = _dialog.find('#apply-to-remaining').is(":checked");

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
					});

					$(_dialog).on("click", ".btn-cancel", function () {
						_dialog.data('cancelled', true);
						self.close();
					});

					$(_dialog).on("hidden.bs.modal", function () {
						if (_.isFunction(self.onComplete)) {
							self.onComplete.apply(self, [_dialog.data('cancelled')]);
                        }

						_dialog.trigger("resolution-complete");

						self.currentIndex = 0;
						self.callerId = null;
						self.queue = null;
						self.onResolve = _.noop;
						self.onComplete = _.noop;
					});

					onReady.apply(_dialog);
				}
			});
		};
	};

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
					thumbUrl: dzfile.dataURL,
					name: dzfile.name,
					title: title,
					ext: ext,
					mime: dzfile.type,
					type: mediaType,
					createdOn: dzfile.lastModifiedDate,
					width: dzfile.width,
					height: dzfile.height,
					size: dzfile.size,
					dimensions: dzfile.width + ", " + dzfile.height
				};

				return { source: file, dest: dzfile.media };
			});
		},
		fileConflictResolutionDialog: new FileConflictResolutionDialog()
	};
})();