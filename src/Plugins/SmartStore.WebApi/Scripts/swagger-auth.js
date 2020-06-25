(function () {

	$(function () {
		var SmNetHmacSigner = function (name) {
			this.name = name;
		};

		SmNetHmacSigner.prototype.apply = function (obj, authorizations) {

			if (!CryptoJS || typeof(CryptoJS.HmacSHA256) !== 'function') {
				console.log('CryptoJS.HmacSHA256 unavailable!');
				return true;
			}

			var publicKey = $('#input_publickey').val(),
				secretKey = $('#input_secretkey').val();

			// Add headers required for Smartstore HMAC authentication
			if (publicKey && publicKey.trim() != '' && secretKey && secretKey.trim() != '') {

				smNetConsumer.init({
					publicKey: publicKey,
					secretKey: secretKey
				});

				if ((obj.method || 'GET') === 'GET' && obj.url.indexOf('$top=') === -1)
					obj.url += (obj.url.indexOf('?') === -1 ? '?' : '@') + '$top=120';

				var options = {
					url: obj.url,
					type: obj.method || 'GET',
					accepts: obj.headers['Accept'] || 'application/json'
				};

				var now = new Date(),
					timestamp = now.toISOString(),
					contentMd5Hash = null,
					data = obj.body;

				if (typeof (data) === 'object')
					data = JSON.stringify(data);

				contentMd5Hash = smNetConsumer.createContentMd5Hash(data);

				var messageRepresentation = smNetConsumer.createMessageRepresentation(contentMd5Hash, timestamp, options),
					signature = smNetConsumer.createSignature(messageRepresentation);

				obj.headers['SmartStore-Net-Api-PublicKey'] = publicKey;
				obj.headers['SmartStore-Net-Api-Date'] = timestamp;
				obj.headers['Authorization'] = smNetConsumer.createAuthorizationHeader(signature);
			}		

			return true;
		};

		if ($('#input_publickey').length === 0) {
			// hide original key input and explore button
			$('#input_apiKey').hide();
			$('#explore').hide();

			// add public and secret key inputs at top of document
			var authUi =
				'<div class="input"><input placeholder="Public-Key" id="input_publickey" name="publickey" type="text" style="width:220px;"></div>' +
				'<div class="input"><input placeholder="Secret-Key" id="input_secretkey" name="secretkey" type="text" style="width:220px;"></div>';

			$(authUi).insertBefore('#api_selector div.input:last-child');

			// Add Smartstore HMAC authentication
			swaggerUi.api.clientAuthorizations.add('SmNetHmac1', new SmNetHmacSigner('SmNetHmac1'));
		}
	});

})();