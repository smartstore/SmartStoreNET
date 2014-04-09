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
    	skin: 'smartstore',
    	body_class: 'mce-content-body-smartstore',
        valid_children: "+body[style]",
        convert_urls: false,
        relative_urls: false,
        resize: 'both',
        plugins: tiny_mce_plugins.join(","),
		image_advtab: true,
        toolbar: "undo redo | styleselect | bold italic underline | alignleft aligncenter alignright alignjustify | bullist numlist outdent indent | forecolor backcolor | link image jbimages | fullscreen"
    };


})();