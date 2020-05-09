
SmartStore.media = (function () {
    var img = { name: "far fa-file-image", color: "#e77c00" };
    var video = { name: "far fa-file-video", color: "#ff5722" };
    var word = { name: "far fa-file-word", color: "#2b579a" };
    var excel = { name: "far fa-file-excel", color: "#217346" };
    var ppt = { name: "far fa-file-powerpoint", color: "#d24726" };
    var pdf = { name: "far fa-file-pdf", color: "#f44336" };
    var zip = { name: "far fa-file-archive", color: "#3f51b5" };
    var csv = { name: "fas fa-file-csv", color: "#607d8b" };
    var markup = { name: "far fa-file-code", color: "#4caf50" };
    var app = { name: "fa fa-cog", color: "#58595b" };
    var db = { name: "fa fa-database", color: "#3ba074" };
    var font = { name: "fa fa-font", color: "#797985" };
    var code = { name: "fa fa-bolt", color: "#4caf50" };

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
        ".swf": video,
        // Media types
        "image": img,
        "video": video,
        "audio": { name: "far fa-file-audio", color: "#009688" },
        "document": { name: "fas fa-file-alt", color: "#2b579a" },
        "text": { name: "far fa-file-alt", color: "#607d8B" },
        "bin": { name: "far fa-file", color: "#bbb" },
        // Rescue
        "misc": { name: "far fa-file", color: "#bbb" },
    };

	return {
		getIconHint: function (file) {
            return iconHints[file.ext] || iconHints[file.type] || iconHints['misc'];
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
