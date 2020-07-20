;

var summernote_global_config;
var summernote_image_upload_url;

(function () {
    var dom = $.summernote.dom;
    var originalDomHtml = dom.html;

    var beautifyOpts = {
        indent_size: 2,
        indent_with_tabs: true,
        indent_char: " ",
        max_preserve_newlines: "2",
        preserve_newlines: true,
        keep_array_indentation: false,
        break_chained_methods: false,
        indent_scripts: "normal",
        brace_style: "collapse",
        space_before_conditional: true,
        unescape_strings: false,
        jslint_happy: false,
        end_with_newline: false,
        wrap_line_length: "140",
        indent_inner_html: true,
        comma_first: false,
        e4x: false,
        indent_empty_lines: false
    };

    dom.html = function ($node, isNewlineOnBlock) {
        var markup = dom.value($node);
        if (isNewlineOnBlock) {
            markup = window.html_beautify(markup, beautifyOpts);
        }
        return markup;
    };

	$.extend(true, $.summernote.lang, {
		'en-US': {
			attrs: {
				cssClass: 'CSS Class',
				cssStyle: 'CSS Style',
				rel: 'Rel',
			},
			link: {
				browse: 'Browse'
			},
			image: {
				imageProps: 'Image Attributes'
			},
			imageShapes: {
				tooltip: 'Shape',
				tooltipShapeOptions: ['Responsive', 'Border', 'Rounded', 'Circle', 'Thumbnail', 'Shadow (small)', 'Shadow (medium)', 'Shadow (large)']
			}
		}
	});

	summernote_global_config = {
		disableDragAndDrop: false,
		dialogsInBody: true,
		dialogsFade: true,
		height: 300,
		prettifyHtml: true,
		onCreateLink: function (url) {
			// Prevents that summernote prepends "http://" to our links (WTF!!!)
			var c = url[0];
			if (c === "/" || c === "~" || c === "\\" || c === "." || c === "#") {
				return url;
			}

			if (/^[A-Za-z][A-Za-z0-9+-.]*\:[\/\/]?/.test(url)) {
				// starts with a valid protocol
				return url;
			}

			// if url doesn't match an URL schema, set http:// as default
			return "http://" + url;
		},
        callbacks: {
			onFocus: function () {
				$(this).next().addClass('focus');
			},
			onBlur: function (e) {
				var inside = $(e.relatedTarget).closest('.note-editable').length || $(e.relatedTarget).closest('.note-popover').length;
				if (!inside) {
					 // Close all popovers
					_.delay(function () { $('.note-popover').hide(); }, 50);

					$(this).next().removeClass('focus');
				}
			},
			onImageUpload: function (files) {
				sendFile(files[0], this);
			},
            onBlurCodeview: function (code, e) {
				// Summernote does not update WYSIWYG content on codable blur,
				// only when switched back to editor
                $(this).val(code);
            }
		},
		toolbar: [
			['text', ['bold', 'italic', 'underline', 'strikethrough', 'clear', 'cleaner']],
			['font', ['fontname', 'color', 'fontsize']],
			['para', ['style', 'cssclass', 'ul', 'ol', 'paragraph']],
			['insert', ['link', 'media',  'table', 'hr', 'video']],
			['view', ['fullscreen', 'codeview', 'help']]
		],
		popover: {
			image: [
				['custom', ['imageAttributes', 'link', 'unlinkImage', 'imageShapes']],
				['imagesize', ['imageSize100', 'imageSize50', 'imageSize25']],
				//['float', ['floatLeft', 'floatRight', 'floatNone']],
				['float', ['bsFloatLeft', 'bsFloatRight', 'bsFloatNone']],
				['remove', ['removeMedia']]
			],
            link: [
                ['link', ['linkDialogShow', 'unlink']]
            ],
            table: [
                ['add', ['addRowDown', 'addRowUp', 'addColLeft', 'addColRight']],
				['delete', ['deleteRow', 'deleteCol', 'deleteTable']],
				['custom', ['tableStyles']]
            ],
            air: [
                ['color', ['color']],
                ['font', ['bold', 'underline', 'clear']],
                ['para', ['ul', 'paragraph']],
                ['table', ['table']],
                ['insert', ['link', 'picture']]
            ]
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
			'circle': 'far fa-circle',
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
			'pencil': 'fa fa-pencil-alt',
			'picture': 'far fa-image',
			'question': 'fa fa-question',
			'redo': 'fa fa-redo',
			'square': 'far fa-square',
			'strikethrough': 'fa fa-strikethrough',
			'subscript': 'fa fa-subscript',
			'superscript': 'fa fa-superscript',
			'table': 'fa fa-table',
			'textHeight': 'fa fa-text-height',
			'trash': 'fa fa-trash',
			'underline': 'fa fa-underline',
			'undo': 'fa fa-undo',
			'unorderedlist': 'fa fa-list-ul',
			'video': 'fa fa-video'
		},
		codemirror: {
			mode: "htmlmixed",
			theme: "eclipse",
			lineNumbers: true,
			lineWrapping: false,
            tabSize: 2,
            indentWithTabs: true,
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
            icon: '<i class="fa fa-pencil-alt"/>',
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

	// Custom events
	$(function () {
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

		// Fix "CodeMirror too wide" issue
		$(document).on('click', '.note-toolbar .btn-codeview', function (e) {
            var wrapper = $(this).closest('.adminData');
            if (wrapper.length) {
				wrapper.css('overflow-x', $(this).is('.active') ? 'auto' : '');
			}
		});
	});
})();