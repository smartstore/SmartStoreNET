;

var summernote_global_config;
var summernote_image_upload_url;

(function () {

	summernote_global_config = {
		codemirror: {
			theme: 'default'
		},
		height: 300,
		onImageUpload: function (files, editor, welEditable) {
			sendFile(files[0], editor, welEditable);
		}
	};

	function sendFile(file, editor, welEditable) {
		data = new FormData();
		data.append("file", file);
		$.ajax({
			data: data,
			type: "POST",
			url: summernote_image_upload_url,
			cache: false,
			contentType: false,
			processData: false,
			success: function (result) {
				console.log(result);
				if (result.Success) {
					editor.insertImage(welEditable, result.Url);
				}
				else {
					EventBroker.publish("message", {
						title: 'Image upload error',
						text: result.Message,
						type: 'error',
						hide: false
					})
				}
			}
		});
	}

})();