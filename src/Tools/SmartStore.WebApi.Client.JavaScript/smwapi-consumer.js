
(function (smNetConsumer, $, undefined) {

	smNetConsumer.settings = {
		publicKey: '',
		secretKey: '',
		url: '',
		odataService: '/odata/v1',
		apiService: '/api/v1',
		httpAcceptType: 'application/json, text/javascript, */*'
	};

	smNetConsumer.init = function (settings) {
		$.extend(this.settings, settings);

		ensureIso8601Date();
	};

	smNetConsumer.createContentMd5Hash = function (content) {
		if (content && content.length > 0) {
			var hash = CryptoJS.MD5(content);

			return CryptoJS.enc.Base64.stringify(hash);
		}
		return '';
	};

	smNetConsumer.createMessageRepresentation = function (contentMd5Hash, timestamp, ajaxOptions) {
		var result = [
			ajaxOptions.type.toLowerCase(),
			contentMd5Hash || '',
			ajaxOptions.accepts.toLowerCase(),
			ajaxOptions.url.toLowerCase(),
			timestamp,
			this.settings.publicKey.toLowerCase()
		].join('\n');

		return result;
	};

	smNetConsumer.createSignature = function (messageRepresentation) {
		var hash = CryptoJS.HmacSHA256(messageRepresentation, this.settings.secretKey),
			signature = CryptoJS.enc.Base64.stringify(hash);

		return signature;
	};

	smNetConsumer.createAuthorizationHeader = function (signature) {
		if (!signature || signature.length <= 0)
			return '';

		return 'SmNetHmac1 ' + signature;
	};

	smNetConsumer.startRequest = function (options) {
		var now = new Date(),
			timestamp = now.toISOString(),
			contentMd5Hash = null;

		var ajaxOptions = {
			url: this.settings.url + options.resource,
			type: options.method || 'GET',
			accepts: this.settings.httpAcceptType,
			headers: {
				"Accept": this.settings.httpAcceptType,
				"SmartStore-Net-Api-PublicKey": this.settings.publicKey,
				"SmartStore-Net-Api-Date": timestamp
			},
			beforeSend: function (jqXHR, settings) {
				Callback(options.beforeSend, jqXHR, settings);
			}
		};

		if (options.content) {
			var data = options.content;

			if (typeof(data) === 'object')
				data = JSON.stringify(data);
			
			contentMd5Hash = this.createContentMd5Hash(data);

			$.extend(ajaxOptions, {
				contentType: 'application/json; charset=utf-8',
				dataType: 'json',
				data: data
			});

			$.extend(ajaxOptions.headers, {
				"Content-MD5": contentMd5Hash		// optional
			});
		}

		var messageRepresentation = this.createMessageRepresentation(contentMd5Hash, timestamp, ajaxOptions),
			signature = this.createSignature(messageRepresentation);

		$.extend(ajaxOptions.headers, {
			"Authorization": this.createAuthorizationHeader(signature)
		});

		$.ajax(ajaxOptions)
			.done(function (data, textStatus, jqXHR) {
				Callback(options.done, data, textStatus, jqXHR);
			})
			.fail(function (jqXHR, textStatus, errorThrown) {
				Callback(options.fail, jqXHR, textStatus, errorThrown);
			});
	}


	// see https://developer.mozilla.org/en-US/docs/JavaScript/Reference/Global_Objects/Date/toISOString
	function ensureIso8601Date() {
		if (Date.prototype.toISOString)
			return;

		function pad(number) {
			return (number < 10 ? '0' + number : number);
		}

		// fallback
		Date.prototype.toISOString = function () {
			return this.getUTCFullYear() +
				'-' + pad(this.getUTCMonth() + 1) +
				'-' + pad(this.getUTCDate()) +
				'T' + pad(this.getUTCHours()) +
				':' + pad(this.getUTCMinutes()) +
				':' + pad(this.getUTCSeconds()) +
				'.' + (this.getUTCMilliseconds() / 1000).toFixed(3).slice(2, 5) +
				'Z';
		};
	}

	function Callback(func) {
		if (typeof func === 'function')
			return func.apply(this, Array.prototype.slice.call(arguments, 1));
		return null;
	}

}(window.smNetConsumer = window.smNetConsumer || {}, jQuery));