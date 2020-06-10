SmartStore.Admin.Media = (function () {
	return {
		convertDropzoneFileQueue: function (queue) {
			var returnObj = [];

			for (var file of queue) {
				var convertedFile = {
					thumbUrl: file.dataURL,
					name: file.name,
					createdOn: moment(file.lastModifiedDate).format('L LTS'),
					width: file.width,
					height: file.height,
					size: file.size
				};

				var mapped = { source: convertedFile, dest: file.media };
				returnObj.push(mapped);
			}

			return returnObj;
		}
	};
})();

function FileConflictResolutionDialog(options) {
	var self = this;

	// Private variables.
	var _dialog = $("#duplicate-window");
	var _dupeFileDisplay = _dialog.find(".dupe-file-display");

	this.currentIndex = 0;

	// Public variables.
	Object.defineProperty(this, 'currentFile', {
		get: function () { return this.queue[this.currentIndex]; },
		//set: function (v) { currentFile = v; }
	});

	//Object.defineProperty(this, 'currentIndex', {
	//	get: function () { return this._currentIndex; },
	//	set: function (v) {
	//		currentFile = this.queue[v];
	//		currentIndex = v;
	//	}
	//});

	Object.defineProperty(this, 'isOpen', {
		get: function () { return _dialog && _dialog.hasClass("show"); }
	});

	this.url = options.url;
	this.callerId = options.callerId;
	this.queue = options.queue;
	this.onResolve = options.onResolve || _.noop;			// return params => self, dupeFileHandlingType, saveSelection, files
	this.onComplete = options.onComplete || _.noop;		// return params => self, isCanceled
	this.closeOnCompleted = toBool(options.closeOnCompleted, true);

	// Public functions.
	this.open = function () {
		this.currentIndex = 0;

		var onReady = function () {
			_dialog.modal('show');
			self.refresh();
		};

		if (_dialog.length === 0) {
			initDialog(self.url, onReady);
		}
		else {
			onReady();
		}
	};

	this.hide = function () {
		_dialog.modal('hide');
	};

	this.next = function () {
		this.currentIndex++;
		this.refresh();

		// End of queue is reached.
		if (this.currentIndex === this.queue.length) {
			if (this.closeOnCompleted)
				this.hide();

			if (this.onComplete)
				this.onComplete.apply(this, [false]);

			this.currentIndex = 0;
		}
	};

	// Private functions.
	this.refresh = function () {
		var existingFileDisplay = _dialog.find(".existing-file-display");
		var source = this.currentFile.source;
		var dest = this.currentFile.dest;

		// Display current filename in intro text.
		_dialog.find(".intro .current-file").text(source.name);

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
		el.find(".file-date").text(file.createdOn);
		el.find(".file-size").text(_.formatFileSize(file.size));

		if (file.width && file.height) {
			el.find(".file-dimensions").text(file.width + " x " + file.height);
		}
	};

	var initDialog = function (url, onReady) {
		// Get dialog via ajax and append to body.
		$.ajax({
			async: true,
			cache: false,
			type: 'POST',
			url: url,
			success: function (response) {
				$("body").append($(response));
				_dialog = $("#duplicate-window");
				_dupeFileDisplay = _dialog.find(".dupe-file-display");

				// Listen to change events of radio group (dupe handling type) and display name of renamed file accordingly.
				$(_dialog).on("change", 'input[name=resolution-type]', function (e) {
					var fileName = self.currentFile.dest.name;

					if ($(e.target).val() === "2") {
						var newPath = self.currentFile.dest.newPath;
						fileName = newPath.substr(newPath.lastIndexOf("/") + 1);
					}

					_dupeFileDisplay.find(".file-name").text(fileName);
				});

				$(_dialog).on("click", ".start-upload", function () {
					var resolutionType = _dialog.find('input[name=resolution-type]:checked').val();
					var applyToRemaining = _dialog.find('#apply-to-remaining').is(":checked");

					if (self.onResolve) {
						var start = self.currentIndex;
						var end = applyToRemaining ? self.queue.length - 1 : self.currentIndex + 1;
						//var nextQueue = applyToRemaining ? null : [self.currentFile];
						var remainingFiles = self.queue.slice(start, end);
						self.onResolve.apply(self, [resolutionType, remainingFiles]);
					}
				});

				$(_dialog).on("click", ".cancel-upload", function () {
					self.hide();

					if (self.onComplete)
						self.onComplete.apply(self, [true]);
				});

				onReady();
			}
		});
	};
};