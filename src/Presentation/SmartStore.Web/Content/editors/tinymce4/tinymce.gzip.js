(function () {
	var tinymce, loaded = {}, urls = [], callbacks = [], loading, realInit;

	function loadScript(url, callback) {
		var elm, head = document.getElementsByTagName('head')[0] || document.body;

		// Execute callback when script is loaded
		function done() {
			elm.parentNode.removeChild(elm);

			if (elm) {
				elm.onreadystatechange = elm.onload = elm = null;
			}

			callback();
		}

		function error() {
			// Report the error so it's easier for people to spot loading errors
			if (typeof (console) !== "undefined" && console.log) {
				console.log("Failed to load: " + url);
			}
		}

		// Create new script element
		elm = document.createElement('script');
		elm.type = 'text/javascript';

		// Use modern onload event when it's available
		if ("onload" in elm) {
			elm.onload = done;
			head.appendChild(elm);

			// Add src after appending it to the document seems to work better on IE 10
			elm.src = url;
		} else {
			elm.onreadystatechange = function () {
				if (elm.src && /loaded|complete/.test(elm.readyState)) {
					done();
				}
			};

			elm.src = url;
			head.appendChild(elm);
		}

		elm.onerror = error;
	}

	function buildUrl(themes, plugins, languages) {
		var url = '';

		function getQueryPart(type, items) {
			if (items) {
				for (var i = items.length - 1; i >= 0; i--) {
					if (loaded[type + '_' + items[i]]) {
						items.splice(i, 1);
					} else {
						loaded[type + '_' + items[i]] = true;
					}
				}

				if (items.length) {
					return '&' + type + 's=' + items.join(',');
				}
			}

			return '';
		}

		url += getQueryPart("plugin", plugins);
		url += getQueryPart("theme", themes);
		url += getQueryPart("language", languages);

		if (url) {
			if (loaded.core) {
				url += '&core=false';
			} else {
				loaded.core = true;
			}

			url = tinymce.baseURL + '/tinymce.gzip.ashx?js=true' + url;
		}

		return url;
	}

	function splitValue(value) {
		if (typeof (value) == "string") {
			return value.split(/[, ]/);
		}

		var items = [];

		if (value) {
			for (var i = 0; i < value.length; i++) {
				items = items.concat(splitValue(value[i]));
			}
		}

		return items;
	}

	function loadNext() {
		var url = urls.shift();

		if (!url) {
			for (var i = 0; i < callbacks.length; i++) {
				callbacks[i]();
			}

			callbacks = [];
			loading = false;
		} else {
			loadScript(url, loadNext);
		}
	}

	function init(settings) {
		var themes = [], plugins = [], languages = [];

		themes.push(settings.theme || 'modern');

		var pluginList = splitValue(settings.plugins);
		for (var i = 0; i < pluginList.length; i++) {
			plugins.push(pluginList[i]);
		}

		if (settings.language) {
			languages.push(settings.language);
		}

		urls.push(buildUrl(themes, plugins, languages));

		callbacks.push(function () {
			if (/complete|interactive/.test(document.readyState)) {
				window.tinymce.dom.Event.domLoaded = true;
			}

			if (window.tinymce.init != init) {
				realInit = window.tinymce.init;
				window.tinymce.init = init;
			}

			realInit.call(window.tinymce, settings);
		});

		if (!loading) {
			loading = true;
			loadNext();
		}
	}

	function getBaseUrl() {
		var scripts = document.getElementsByTagName('script');
		for (var i = 0; i < scripts.length; i++) {
			var src = scripts[i].src;

			if (src.indexOf('tinymce.gzip.js') != -1) {
				return src.substring(0, src.lastIndexOf('/'));
			}
		}
	}

	tinymce = {
		init: init,
		baseURL: getBaseUrl(),
		suffix: ".min"
	};

	window.tinyMCE_GZ = {
		init: function (settings, callback) {
			urls.push(buildUrl(splitValue(settings.themes), splitValue(settings.plugins), splitValue(settings.languages)));

			callbacks.push(function () {
				window.tinymce.dom.Event.domLoaded = 1;
				callback();
			});

			if (!loading) {
				loading = true;
				loadNext();
			}
		}
	};

	window.tinymce = window.tinyMCE = tinymce;
})();