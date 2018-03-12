;

var summernote_global_config;
var summernote_image_upload_url;

(function () {
	// Editor toggling
	$(document).on('click', '.note-editor-preview', function (e) {
		var div = $(this);
		var textarea = $(div.data("target"));
		var lang = div.data("lang");

		div.remove();
		textarea
			.removeClass('d-none')
			.summernote($.extend(true, {}, summernote_global_config, { lang: lang, focus: true }));

	});

	summernote_global_config = {
		disableDragAndDrop: false,
		dialogsInBody: false,
		dialogsFade: true,
		height: 300,
		onCreateLink: function (url) {
			// Prevents that summernote prepends "http://" to our links (WTF!!!)
			if (url[0] == "/" || url[0] == "~" || url[0] == "\\" || url[0] == "." || url[0] == "#") {
				return url;
			}

			return "http://" + url;
		},
		callbacks: {
			onFocus: function () {
				$(this).next().addClass('focus');
			},
			onBlur: function () {
				$(this).next().removeClass('focus');
			},
			onImageUpload: function (files) {
				sendFile(files[0], this);
			}
		},
		toolbar: [
			['text', ['bold', 'italic', 'underline', 'strikethrough', 'clear', 'cleaner']],
			['font', ['fontname', 'color', 'fontsize']],
			['para', ['style', 'ul', 'ol', 'paragraph']],
			['insert', ['link', 'media',  'table', 'hr', 'video']],
			['view', ['fullscreen', 'codeview', 'help']]
		],
		popover: {
			image: [
				['custom', ['imageAttributes']],
				['imagesize', ['imageSize100', 'imageSize50', 'imageSize25']],
				['float', ['floatLeft', 'floatRight', 'floatNone']],
				['remove', ['removeMedia']]
			],
		},
		icons: {
			'align': 'fa fa-align-left',
			'alignCenter': 'fa fa-align-center',
			'alignJustify': 'fa fa-align-justify',
			'alignLeft': 'fa fa-align-left',
			'alignRight': 'fa fa-align-right',
			//'rowBelow': 'note-icon-row-below',
			//'colBefore': 'note-icon-col-before',
			//'colAfter': 'note-icon-col-after',
			//'rowAbove': 'note-icon-row-above',
			//'rowRemove': 'note-icon-row-remove',
			//'colRemove': 'note-icon-col-remove',
			'indent': 'fa fa-indent',
			'outdent': 'fa fa-outdent',
			'arrowsAlt': 'fa fa-arrows-alt',
			'bold': 'fa fa-bold',
			'caret': 'fa fa-caret-down',
			'circle': 'fa fa-circle-thin',
			'close': 'fa fa-times',
			'code': 'fa fa-code',
			'eraser': 'fa fa-eraser',
			'font': 'fa fa-font',
			//'frame': 'note-icon-frame',
			'italic': 'fa fa-italic',
			'link': 'fa fa-link',
			'unlink': 'fa fa-unlink',
			'magic': 'fa fa-magic', // magic
			'menuCheck': 'fa fa-check',
			'minus': 'fa fa-minus',
			'orderedlist': 'fa fa-list-ol',
			'pencil': 'fa fa-pencil',
			'picture': 'fa fa-picture-o',
			'question': 'fa fa-question',
			'redo': 'fa fa-redo',
			'square': 'fa fa-square-o',
			'strikethrough': 'fa fa-strikethrough',
			'subscript': 'fa fa-subscript',
			'superscript': 'fa fa-superscript',
			'table': 'fa fa-table',
			'textHeight': 'fa fa-text-height',
			'trash': 'fa fa-trash',
			'underline': 'fa fa-underline',
			'undo': 'fa fa-undo',
			'unorderedlist': 'fa fa-list-ul',
			'video': 'fa fa-video-camera'
		},
		codemirror: {
			mode: "htmlmixed",
			theme: "eclipse",
			lineNumbers: true,
			lineWrapping: true,
			tabSize: 2,
			smartIndent: true,
			matchTags: true,
			matchBrackets: true,
			autoCloseTags: true,
			autoCloseBrackets: true,
			styleActiveLine: true,
			extraKeys: {
				"'.'": CodeMirror.hint.completeAfter,
				"'<'": CodeMirror.hint.completeAfter,
				"'/'": CodeMirror.hint.completeIfAfterLt,
				"' '": CodeMirror.hint.completeIfAfterSpace,
				"'='": CodeMirror.hint.completeIfInTag,
				"Ctrl-Space": "autocomplete",
				"F11": function (cm) { cm.setOption("fullScreen", !cm.getOption("fullScreen")); },
				"Esc": function (cm) { if (cm.getOption("fullScreen")) cm.setOption("fullScreen", false); }
			},
			hintOptions: {
				closeCharacters: /[\s()\[\]{};:>,.|%]/,
				completeSingle: false
			}
		},
		imageAttributes: {
			icon: '<i class="fa fa-pencil"/>',
			removeEmpty: true, // true = remove attributes | false = leave empty if present
			disableUpload: true // true = don't display Upload Options | Display Upload Options
		}
	};

	function sendFile(file, editor, welEditable) {
		data = new FormData();
		data.append("file", file);
		data.append("a", "UPLOAD");
		data.append("d", "Uploaded");
		data.append("ext", true);
		$.ajax({
			data: data,
			type: "POST",
			url: summernote_image_upload_url,
			cache: false,
			contentType: false,
			processData: false,
			success: function (result) {
				if (result.Success) {
					$(editor).summernote('insertImage', result.Url);
				}
				else {
					EventBroker.publish("message", {
						title: 'Image upload error',
						text: result.Message,
						type: 'error',
						hide: false
					});
				}
			}
		});
	}
})();