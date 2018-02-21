
; (function () {
	// Overwrite core parts of CKEditor in order to be able to
	// load Sass files instead of static css
	var cssLoaded = {};

	CKEDITOR.skin.loadPart = function (part, fn) {
		if (CKEDITOR.skin.name != "smartstore") {
			CKEDITOR.scriptLoader.load(CKEDITOR.skin.path() + 'skin.js', function () {
				loadCss(part, fn);
			});
		} else {
			loadCss(part, fn);
		}
	};

	function loadCss(part, callback) {
		// Avoid reload.
		if (!cssLoaded[part]) {
			CKEDITOR.document.appendStyleSheet(CKEDITOR.skin.getPath(part));
			cssLoaded[part] = 1;
		}

		// CSS loading should not be blocking.
		callback && callback();
	}

	CKEDITOR.skin.getPath = function (part) {
		return CKEDITOR.skin.path() + part + ".scss";
	};
})();
