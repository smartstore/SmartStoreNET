(function ($, window, document, undefined) {

	// TODO: Implement ValidationAttributes in SmartStore.Validation namespace

	// FileExtensions validation
	$.validator.unobtrusive.adapters.add('fileextensions', ['extensions'], function (options) {
		var params = {
			extensions: options.params.extensions.split(',')
		};

		options.rules['fileextensions'] = params;
		if (options.message) {
			options.messages['fileextensions'] = options.message;
		}
	});

	$.validator.addMethod("fileextensions", function (value, element, param) {
		if (!value)
			return true;

		var extension = getFileExtension(value);
		var validExtension = $.inArray(extension, param.extensions) !== -1;
		return validExtension;
	});

	function getFileExtension(fileName) {
		var extension = (/[.]/.exec(fileName)) ? /[^.]+$/.exec(fileName) : undefined;
		if (extension != undefined) {
			return extension[0];
		}
		return extension;
	};


	// FileSize validation
	jQuery.validator.unobtrusive.adapters.add('filesize', ['maxbytes'], function (options) {
		// Set up test parameters
		var params = {
			maxbytes: options.params.maxbytes
		};

		// Match parameters to the method to execute
		options.rules['filesize'] = params;
		if (options.message) {
			// If there is a message, set it for the rule
			options.messages['filesize'] = options.message;
		}
	});

	jQuery.validator.addMethod("filesize", function (value, element, param) {
		if (value === "") {
			// no file supplied
			return true;
		}

		var maxBytes = parseInt(param.maxbytes);

		// use HTML5 File API to check selected file size
		// https://developer.mozilla.org/en-US/docs/Using_files_from_web_applications
		// http://caniuse.com/#feat=fileapi
		if (element.files != undefined && element.files[0] != undefined && element.files[0].size != undefined) {
			var filesize = parseInt(element.files[0].size);

			return filesize <= maxBytes;
		}

		// if the browser doesn't support the HTML5 file API, just return true
		// since returning false would prevent submitting the form 
		return true;
	});


	// MustBeTrue validation
	jQuery.validator.unobtrusive.adapters.addBool("mustbetrue");
	jQuery.validator.addMethod("mustbetrue", function (value, element, param) {
		return element.checked;
	});



	// Validator <> Bootstrap
	function setControlFeedback(ctl, success) {
		if (ctl.is(':checkbox') || ctl.is(':radio')) {
			return;
		}

		if (success) {
			ctl.addClass('is-valid')
				.removeClass('is-invalid')
				.find('+ span[data-valmsg-for]')
				.addClass('valid-feedback');
		}
		else {
			ctl.removeClass('is-valid')
				.addClass('is-invalid')
				.find('+ span[data-valmsg-for]')
				.addClass('invalid-feedback');
		}
	}

	$.validator.setDefaults({
		onfocusout: function (el, e) {
			if ($(el).is(".is-valid, .is-invalid")) {
				$(el).valid();
			}
		},
		onkeyup: function (el, e) {
			if ($(el).is(".is-valid, .is-invalid")) {
				$(el).valid();
			}
		},
		onclick: false,
		highlight: function (el, errorClass, validClass) {
			//$(el).addClass('is-invalid').removeClass('is-valid');
			setControlFeedback($(el), false);
		},
		unhighlight: function (el, errorClass, validClass) {
			if ($(el).is(".is-invalid")) {
				setControlFeedback($(el), true);
			}
		}
	});

})(jQuery, this, document);
//# sourceMappingURL=jquery.validate.unobtrusive.custom.js.map