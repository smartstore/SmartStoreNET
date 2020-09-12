
SmartStore.media = (function () {
    var img = { name: "far fa-file-image", color: "#e77c00", mediaType: 'image' };
    var video = { name: "far fa-file-video", color: "#ff5722", mediaType: 'video' };
    var word = { name: "far fa-file-word", color: "#2b579a", mediaType: 'document' };
    var excel = { name: "far fa-file-excel", color: "#217346", mediaType: 'document' };
    var ppt = { name: "far fa-file-powerpoint", color: "#d24726", mediaType: 'document' };
    var pdf = { name: "far fa-file-pdf", color: "#f44336", mediaType: 'document' };
    var zip = { name: "far fa-file-archive", color: "#3f51b5", mediaType: 'bin' };
    var csv = { name: "fas fa-file-csv", color: "#607d8b", mediaType: 'text' };
    var markup = { name: "far fa-file-code", color: "#4caf50", mediaType: 'text' };
    var app = { name: "fa fa-cog", color: "#58595b", mediaType: 'bin' };
    var db = { name: "fa fa-database", color: "#3ba074", mediaType: 'bin' };
    var font = { name: "fa fa-font", color: "#797985", mediaType: 'bin' };
    var code = { name: "fa fa-bolt", color: "#4caf50", mediaType: 'text' };

    var iconHints = {
        // Common extensions
        ".pdf": pdf,
        ".psd": img,
        ".exe": app, ".dll": app,
        ".doc": word, ".docx": word, ".docm": word, ".odt": word, ".dot": word, ".dotx": word, ".dotm": word,
        ".ppt": ppt, ".pptx": ppt, ".pps": ppt, ".ppsx": ppt, ".pptm": ppt, ".odp": ppt, ".potx": ppt, ".pot": ppt, ".potm": ppt, ".ppsm": ppt,
        ".xls": excel, ".xlsx": excel, ".xlsb": excel, ".xlsm": excel, ".ods": excel,
        ".mdb": db, ".db": db, ".sqlite": db,
        ".csv": csv,
        ".zip": zip, ".rar": zip, ".7z": zip,
        ".ttf": font, ".eot": font, ".woff": font, ".woff2": font,
        ".xml": markup, ".html": markup, ".htm": markup,
        ".js": code, ".json": code,
        ".mp4": video, ".webm": video, ".ogg": video, ".swf": video,
        // Media types
        "image": img,
        "video": video,
        "audio": { name: "far fa-file-audio", color: "#009688", mediaType: 'audio' },
        "document": { name: "fas fa-file-alt", color: "#2b579a", mediaType: 'document' },
        "text": { name: "far fa-file-alt", color: "#607d8B", mediaType: 'text' },
        "bin": { name: "far fa-file", color: "#bbb", mediaType: 'bin' },
        // Rescue
        "misc": { name: "far fa-file", color: "#bbb", mediaType: 'bin', isFallback: true }
    };

    return {
        getIconHint: function (file) {
            return iconHints[file.ext] || iconHints[file.type] || (file.mime ? iconHints[file.mime.split('/')[0]] : null) || iconHints['misc'];
        },
        lazyLoadThumbnails: function (selector) {
            $(selector || document).find('img.file-img').each(function () {
                try {
                    var img = $(this);
                    if (img.attr('src') === undefined) {
                        var src = img.data('src');
                        if (src) {
                            img.one('load', function () {
                                img.parent().addClass('show loaded').prev().removeClass('show');
                            });
                            img.prop('src', src);
                        }
                    }
                }
                catch (err) {
                    console.log(err);
                }
            });
        },
        getPreview: function (file, opts) {
            opts = opts || {};

            var o = {
                thumbUrl: file.thumbUrl,
                isImage: file.type === 'image',
                thumbHtml: ''
            };

            var iconHint = this.getIconHint(file);
            var imgCssClasses = !_.isEmpty(opts.imgCssClasses)
                ? 'file-img ' + opts.imgCssClasses
                : 'file-img';

            var iconCssClasses = !_.isEmpty(opts.iconCssClasses)
                ? 'file-icon show fa-fw ' + opts.iconCssClasses + ' '
                : 'file-icon show fa-fw ';

            if (o.isImage) {
                o.thumbHtml = '<img class="' + imgCssClasses + '" title="' + file.title + '" src="' + file.thumbUrl + '" />';
            }
            else {
                // Title must be on the Picture attribute, otherwise it's covered by the CSS play-video symbol.
                o.thumbHtml = '<figure class="file-figure">'
                    + '<i class="' + iconCssClasses + iconHint.name + '" style="color: ' + iconHint.color + '"></i>'
                    + '<picture class="file-thumb" data-type="' + file.type + '" title="' + file.title + '">'
                    + '<img class="' + imgCssClasses + '" data-src="' + file.thumbUrl + '" />'
                    + '</picture>'
                    + '</figure>';
            }

            return o;
        },
        openFileManager: function (opts) {
            /*
                opts = {
                    id: (#modalId) || null,
                    el: (element that triggered this call) || null,
                    backdrop: false,
                    type: (ext || mediaType),
                    multiSelect: false,
                    ???(path: (initialPath) || null),
                    onSelect: function(filesArray) {}
                };
            */

            var url = $(opts.el || document.body).closest('[data-file-manager-url]').attr('data-file-manager-url');
            if (!url)
                return;

            // Append querystring to file manager root url
            if (opts.multiSelect) {
                url = modifyUrl(url, "multiSelect", opts.multiSelect);
            }

            if (opts.type) {
                url = modifyUrl(url, "typeFilter", opts.type);
            }

            opts.id = opts.id || 'modal-file-manager';

            var popup = openPopup({
                id: opts.id,
                url: url,
                backdrop: opts.backdrop,
                flex: true,
                large: true,
                onMessage: function (files) {
                    if (_.isFunction(opts.onSelect)) {
                        opts.onSelect(files);
                    }

                    closePopup(opts.id);
                }
            });

            return popup;
        }
    };
})();
