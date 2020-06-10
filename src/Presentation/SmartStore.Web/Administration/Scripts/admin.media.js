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

				var mapped = { source: convertedFile, target: file.media };
				returnObj.push(mapped);
			}

			return returnObj;
		}
	};
})();

FileConflictResolver = function (options) {
	var self = this;

	// Private variables.
	var _dupeFileHandlerDialog = $("#duplicate-window");
	var _dupeFileDisplay = _dupeFileHandlerDialog.find(".dupe-file-display");

	// Public variables.
	Object.defineProperty(this, 'currentFile', {
		get: function () { return this.queue[this.currentIndex]; },
		set: function (v) { currentFile = v; }
	});

	Object.defineProperty(this, 'currentIndex', {
		get: function () { return currentIndex; },
		set: function (v) {
			currentFile = this.queue[v];
			currentIndex = v;
		}
	});

	Object.defineProperty(this, 'isOpen', {
		get: function () { return _dupeFileHandlerDialog.hasClass("show"); }
	});

	this.url = options.url;
	this.callerId = options.callerId;
	this.queue = options.queue;
	this.onResolve = options.onResolve || function () { };			// return params => self, dupeFileHandlingType, saveSelection, files
	this.onComplete = options.onComplete || function () { };		// return params => self, isCanceled
	this.closeOnCompleted = options.closeOnCompleted || true;

	// Public functions.
	this.open = function () {
		self.currentIndex = 0;

		if (_dupeFileHandlerDialog.length === 0) {
			initDialog(self.url);
		}
		else {
			_dupeFileHandlerDialog.modal('show');
			refresh();
		}
	};

	this.hide = function () {
		_dupeFileHandlerDialog.modal('hide');
	};

	this.next = function () {
		self.currentIndex++;
		refresh();

		// End of queue is reached.
		if (self.currentIndex === self.queue.length) {
			if (self.closeOnCompleted)
				self.hide();

			if (self.onComplete)
				self.onComplete.apply(self, [false]);

			self.currentIndex = 0;
			self.currentFile = null;
		}
	};

	// Private functions.
	var refresh = function () {
		var existingFileDisplay = _dupeFileHandlerDialog.find(".existing-file-display");
		var source = self.currentFile.source;
		var target = self.currentFile.target;

		// Display current filename in intro text.
		_dupeFileHandlerDialog.find(".intro .current-file").text(source.name);

		// Display uploaded file.
		var elIcon = _dupeFileDisplay.find(".file-icon");
		var elImage = _dupeFileDisplay.find(".file-img");

		if (!source.thumbUrl) {
			// Dropzone couldn't fetch a preview for the file currently uploading.
			var icon = SmartStore.media.getIconHint(target);
			elIcon.attr("class", "file-icon fa-4x " + icon.name).css("color", icon.color);
			elImage.addClass("d-none");
		}
		else {
			elIcon.attr("class", "file-icon");
			elImage.attr("src", source.thumbUrl).removeClass("d-none");
		}

		refreshFileDisplay(_dupeFileDisplay, source);

		// Display existing file.
		existingFileDisplay.find(".file-img").attr("src", target.thumbUrl);
		refreshFileDisplay(existingFileDisplay, target);
	};

	var refreshFileDisplay = function (el, file) {
		el.find(".file-name").text(file.name);
		el.find(".file-date").text(file.createdOn);
		el.find(".file-size").text(_.formatFileSize(file.size));

		if (file.width && file.height) {
			el.find(".file-dimensions").text(file.width + " x " + file.height);
		}
	};

	var initDialog = function (url) {
		// Get dialog via ajax and append to body.
		$.ajax({
			async: true,
			cache: false,
			type: 'POST',
			url: url,
			success: function (response) {
				$("body").append($(response));
				_dupeFileHandlerDialog = $("#duplicate-window");
				_dupeFileDisplay = _dupeFileHandlerDialog.find(".dupe-file-display");

				// Display first duplicate.
				refresh();

				// Open dialog.
				_dupeFileHandlerDialog.modal('show');

				// Listen to change events of radio group (dupe handling type) and display name of renamed file accordingly.
				$(_dupeFileHandlerDialog).on("change", 'input[name=resolution-type]', function (e) {
					var fileName = self.currentFile.target.name;

					if ($(e.target).val() === "2") {
						var newPath = self.currentFile.target.newPath;
						fileName = newPath.substr(newPath.lastIndexOf("/") + 1);
					}

					_dupeFileDisplay.find(".file-name").text(fileName);
				});

				$(_dupeFileHandlerDialog).on("click", ".start-upload", function () {
					var resolutionType = _dupeFileHandlerDialog.find('input[name=resolution-type]:checked').val();
					var applyToRemaining = _dupeFileHandlerDialog.find('#apply-to-remaining').is(":checked");

					if (self.onResolve) {
						var remainingFiles = self.queue.slice(self.currentIndex, self.queue.length, self.currentFile);
						self.onResolve.apply(self, [resolutionType, applyToRemaining, remainingFiles]);
					}
				});

				$(_dupeFileHandlerDialog).on("click", ".cancel-upload", function () {
					self.hide();

					if (self.onComplete)
						self.onComplete.apply(self, [true]);
				});
			}
		});
	};
};