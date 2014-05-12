;

var tiny_mce_global_config;

(function () {

    var tiny_mce_plugins = [
        //"autoresize",
        "hr",
        "visualchars",
        "wordcount",
        "textcolor",
        "advlist",
		"autolink",
        "lists",
		"link",
        "image",
        "charmap",
        "print",
        "preview",
        "anchor",
        "searchreplace",
        "visualblocks",
        "code",
        "fullscreen",
        "insertdatetime",
        "media",
        "table",
        "contextmenu",
        "paste",
        "jbimages"
    ]; 

    tiny_mce_global_config = {
    	height: 250,
    	width: '100%',
    	browser_spellcheck : true,
    	skin: 'smartstore',
    	body_class: 'mce-content-body-smartstore',
        valid_children: "+body[style]",
        convert_urls: false,
        relative_urls: false,
        resize: 'both',
        plugins: tiny_mce_plugins.join(","),
        image_advtab: true,
    	convert_fonts_to_spans: true,
        font_formats: "Andale Mono=andale mono,times;"+
			"Arial=arial,helvetica,sans-serif;"+
			"Arial Black=arial black,avant garde;"+
			"Book Antiqua=book antiqua,palatino;"+
			"Comic Sans MS=comic sans ms,sans-serif;"+
			"Courier New=courier new,courier;"+
			"Georgia=georgia,palatino;"+
			"Helvetica=helvetica;"+
			"Impact=impact,chicago;" +
			"Segoe UI=Segoe UI, sans-serif;"+
			"Symbol=symbol;"+
			"Tahoma=tahoma,arial,helvetica,sans-serif;"+
			"Terminal=terminal,monaco;"+
			"Times New Roman=times new roman,times;"+
			"Trebuchet MS=trebuchet ms,geneva;"+
			"Verdana=verdana,geneva;"+
			"Webdings=webdings;"+
			"Wingdings=wingdings,zapf dingbats",
        toolbar: "undo redo | bold italic underline | bullist numlist | outdent indent | alignleft aligncenter alignright | forecolor backcolor | link image jbimages | styleselect fontselect fontsizeselect | fullscreen"
    };


})();