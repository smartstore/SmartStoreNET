;

var tiny_mce_global_config;

(function () {

    var bs_em_selector = "p,h1,h2,h3,h4,h5,h6,td,th,div,ul,ol,li,a,span,sup,sub,button,input,select,textarea,dl,dd,dt,abbr,blockquote,small,address,code,pre,label";

    var tiny_mce_plugins = _.without(
    // include
        [
            "advhr",
            "advimage",
            "advlist",
            "autolink",
            "autoresize",
            "autosave",
            "bbcode",
            "contextmenu",
            "directionality",
            "emotions",
            "fullpage",
            "fullscreen",
            "iespell",
            "inlinepopups",
            "insertdatetime",
            "layer",
            "lists",
            "media",
            "netadvimage",
            "nonbreaking",
            "noneditable",
            "pagebreak",
            "paste",
            "pdw",
            "preview",
            "print",
            "save",
            "searchreplace",
            "spellchecker",
            "style",
            "tabfocus",
            "table",
            "template",
            "visualchars",
            "wordcount",
            "xhtmlxtras"
        ],
    // exclude
        "advhr",
        //"advlist",
        "autolink",
        "autosave",
        "autoresize",
        "bbcode",
        "fullpage",
        //"save",
        "tabfocus"
    ); // _.without

    tiny_mce_global_config = {
        // General options
        mode: "exact",
        theme: "advanced",
        skin: "smartstore",
        height: "350",
        width: "100%",
        verify_html: false,
        ie7_compat: false,

        // Style formats
        style_formats : [
                { title: '.muted', inline: "span", classes: 'muted' },
                { title: '.text-warning', inline: "span", classes: 'text-warning' },
                { title: '.text-error', inline: "span", classes: 'text-error' },
                { title: '.text-info', inline: "span", classes: 'text-info' },
                { title: '.text-success', inline: "span", classes: 'text-success' },

                { title: 'Images' },
                { title: '.img-rounded', selector: 'img', classes: 'img-rounded' },
                { title: '.img-circle', selector: 'img', classes: 'img-circle' },
                { title: '.img-polaroid', selector: 'img', classes: 'img-polaroid' },

                { title: 'Tables' },
                { title: '.table', selector: 'table', classes: 'table' },
                { title: '.table-striped', selector: 'table.table', classes: 'table-striped' },
                { title: '.table-bordered', selector: 'table.table', classes: 'table-bordered' },
                { title: '.table-hover', selector: 'table.table', classes: 'table-hover' },
                { title: '.table-condensed', selector: 'table.table', classes: 'table-condensed' },
                { title: 'tr.success', selector: 'table tr', classes: 'success' },
                { title: 'tr.error', selector: 'table tr', classes: 'error' },
                { title: 'tr.warning', selector: 'table tr', classes: 'warning' },
                { title: 'tr.info', selector: 'table tr', classes: 'info' },

                { title: 'Labels & Badges' },
                { title: '.label', inline: 'span', classes: 'label' },
                { title: '.label.label-success', inline: 'span', classes: 'label label-success' },
                { title: '.label.label-warning', inline: 'span', classes: 'label label-warning' },
                { title: '.label.label-important', inline: 'span', classes: 'label label-important' },
                { title: '.label.label-info', inline: 'span', classes: 'label label-info' },
                { title: '.label.label-inverse', inline: 'span', classes: 'label label-inverse' },
                { title: '.badge', inline: 'span', classes: 'badge' },
                { title: '.badge.badge-success', inline: 'span', classes: 'badge badge-success' },
                { title: '.badge.badge-warning', inline: 'span', classes: 'badge badge-warning' },
                { title: '.badge.badge-important', inline: 'span', classes: 'badge badge-important' },
                { title: '.badge.badge-info', inline: 'span', classes: 'badge badge-info' },
                { title: '.badge.badge-inverse', inline: 'span', classes: 'badge badge-inverse' },

                { title: 'Alerts & Wells' },
                { title: '.alert', block: "div", classes: 'alert' },
                { title: '.alert.alert-error', selector: ".alert", classes: 'alert-error' },
                { title: '.alert.alert-info', selector: ".alert", classes: 'alert-info' },
                { title: '.alert.alert-success', selector: ".alert", classes: 'alert-success' },
                { title: '.well', block: "div", classes: 'well' },
                { title: '.well.well-large', selector: ".well", classes: 'well-large' },
                { title: '.well.alert-small', selector: ".well", classes: 'well-small' },

                { title: 'Block' },
                { title: 'p.lead', selector: 'p', classes: 'lead' },
                { title: 'ul.unstyled', selector: 'ul', classes: 'unstyled' },
                { title: 'dl.dl-horizontal', selector: 'dl', classes: 'dl-horizontal' },

                { title: 'Buttons' },
                { title: '.btn', selector: 'a,button', classes: 'btn' },
                { title: '.btn.btn-primary', selector: 'a,button', classes: 'btn btn-primary' },
                { title: '.btn.btn-info', selector: 'a,button', classes: 'btn btn-info' },
                { title: '.btn.btn-success', selector: 'a,button', classes: 'btn btn-success' },
                { title: '.btn.btn-warning', selector: 'a,button', classes: 'btn btn-warning' },
                { title: '.btn.btn-danger', selector: 'a,button', classes: 'btn btn-danger' },
                { title: '.btn.btn-inverse', selector: 'a,button', classes: 'btn btn-inverse' },
                { title: '.btn-block', selector: 'a.btn,button.btn', classes: 'btn-block' },
                { title: '.btn-large', selector: 'a.btn,button.btn', classes: 'btn-large' },
                { title: '.btn-small', selector: 'a.btn,button.btn', classes: 'btn-small' },
                { title: '.btn-mini', selector: 'a.btn,button.btn', classes: 'btn-mini' }
        ],

        // codehint: sm-add
        plugins: tiny_mce_plugins.join(","),
        pdw_toggle_on: 1,
        pdw_toggle_toolbars: "2,3",
        schema: "html5",
        button_tile_map: true,
        dialog_type : "modal",
        inlinepopups_skin: "clearlooks3",
        valid_children: "+body[style]",

        theme_advanced_buttons1: "pdw_toggle,code,fullscreen,|,undo,redo,|,bold,italic,underline,strikethrough,|,bullist,numlist,|,outdent,indent,|,justifyleft,justifycenter,justifyright,justifyfull,styleselect,formatselect,fontselect,fontsizeselect",
        theme_advanced_buttons2: "cut,copy,paste,pastetext,pasteword,|,search,replace,|,styleprops,|,blockquote,cite,abbr,acronym,del,ins,attribs,|,link,unlink,anchor,netadvimage,cleanup,help,|,insertdate,inserttime,preview,|,forecolor,backcolor",
        theme_advanced_buttons3: "tablecontrols,|,hr,removeformat,visualaid,|,sub,sup,|,charmap,iespell,media,|,print,|,ltr,rtl,|,visualchars,nonbreaking,template,pagebreak", 

        theme_advanced_toolbar_location: "top", //"external",
        theme_advanced_toolbar_align: "left",
        theme_advanced_statusbar_location: "bottom",
        theme_advanced_resizing: true,

        //content_css: "css/content.css",
        convert_urls: false,

        // Drop lists for link/image/media/template dialogs
        template_external_list_url: "lists/template_list.js",
        external_link_list_url: "lists/link_list.js",
        external_image_list_url: "lists/image_list.js",
        media_external_list_url: "lists/media_list.js"
    };

    tinyMCE_GZ.init({
        plugins: tiny_mce_plugins.join(","),
        themes: 'advanced',
        languages: 'en,de',
        disk_cache: false,
        debug: false
    });

})();