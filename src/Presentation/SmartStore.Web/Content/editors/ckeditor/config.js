/**
 * @license Copyright (c) 2003-2014, CKSource - Frederico Knabben. All rights reserved.
 * For licensing, see LICENSE.md or http://ckeditor.com/license
 */

CKEDITOR.dtd.$removeEmpty['span'] = false;
CKEDITOR.dtd.$removeEmpty['i'] = false;

CKEDITOR.stylesSet.add('custom', [
    { name: 'Table', element: 'table', attributes: { 'class': 'table table-bordered' } },
    { name: 'Button', element: 'button', attributes: { 'class': 'btn btn-default' } }
]);

CKEDITOR.editorConfig = function( config ) {
	// Define changes to default configuration here.
	// For complete reference see:
	// http://docs.ckeditor.com/#!/api/CKEDITOR.config

	config.toolbarCanCollapse = true;
	config.toolbarStartupExpanded = true;
	//config.skin = "moono";
	config.skin = "smartstore";
	config.fillEmptyBlocks = false;

	// The toolbar groups arrangement, optimized for two toolbar rows.
    config.toolbarGroups = [
        { name: 'undo' },
		{ name: 'basicstyles', groups: ['basicstyles', 'cleanup'] },
		{ name: 'colors' },
		{ name: 'links' },
        { name: 'align' },
        { name: 'paragraph', groups: ['list', 'indent', 'blocks'] },
        { name: 'insert' },
		{ name: 'forms' },
		'/',
        { name: 'clipboard' },
        { name: 'editing', groups: ['find', 'selection', 'spellchecker'] },
		{ name: 'styles' },
		{ name: 'about' },
        { name: 'others' },
        { name: 'document', groups: ['mode', 'document', 'doctools', 'tools'] },
	];

	// Remove some buttons provided by the standard plugins, which are
	// not needed in the Standard(s) toolbar.
	config.removeButtons = 'About,placeholder';

	// Set the most common block elements.
	config.format_tags = 'p;h1;h2;h3;h4;pre;address;div';

	// Simplify the dialog windows.
	config.removeDialogTabs = 'image:advanced;link:advanced';

    // TODO: add > templates
	//config.extraPlugins = 'divarea,lineutils,widget,codemirror,panelbutton,quicktable,justify,colorbutton,colordialog,find,font,fontawesome,youtube,showblocks,tableresize,zoom,dialogadvtab';

	config.qtRows = 8; // Count of rows in the quicktable (default: 8)
	config.qtColumns = 8; // Count of columns in the quicktable (default: 10)
	config.qtBorder = '1'; // Border of the inserted table (default: '1')
    config.qtWidth = '100%'; // Width of the inserted table (default: '500px')
    config.qtStyle = null; // Content of the style-attribute of the inserted table (default: null)
    config.qtClass = 'table table-bordered'; // Class of the inserted table (default: '')
    config.qtCellPadding = '0'; // Cell padding of the inserted table (default: '1')
    config.qtCellSpacing = '0'; // Cell spacing of the inserted table (default: '1')

	config.removePlugins = 'placeholder';
		
	config.codemirror = {
	    // Set this to the theme you wish to use (codemirror themes)
	    theme: 'eclipse',
	    tabSize: 2,
        lang: 'de',
	    // Whether or not you want to show line numbers
	    lineNumbers: true,
	    // Whether or not you want to use line wrapping
	    lineWrapping: true,
	    // Whether or not you want to highlight matching braces
	    matchBrackets: true,
	    // Whether or not you want tags to automatically close themselves
	    autoCloseTags: true,
	    // Whether or not you want Brackets to automatically close themselves
	    autoCloseBrackets: true,
	    // Whether or not to enable search tools, CTRL+F (Find), CTRL+SHIFT+F (Replace), CTRL+SHIFT+R (Replace All), CTRL+G (Find Next), CTRL+SHIFT+G (Find Previous)
	    enableSearchTools: true,
	    // Whether or not you wish to enable code folding (requires 'lineNumbers' to be set to 'true')
	    enableCodeFolding: true,
	    // Whether or not to enable code formatting
	    enableCodeFormatting: true,
	    // Whether or not to automatically format code should be done when the editor is loaded
	    autoFormatOnStart: true,
	    // Whether or not to automatically format code should be done every time the source view is opened
	    autoFormatOnModeChange: true,
	    // Whether or not to automatically format code which has just been uncommented
	    autoFormatOnUncomment: true,
	    // Whether or not to highlight the currently active line
	    highlightActiveLine: true,
	    // Define the language specific mode 'htmlmixed' for html including (css, xml, javascript), 'application/x-httpd-php' for php mode including html, or 'text/javascript' for using java script only
	    mode: 'htmlmixed',
	    // Whether or not to show the search Code button on the toolbar
	    showSearchButton: true,
	    // Whether or not to show Trailing Spaces
	    showTrailingSpace: true,
	    // Whether or not to highlight all matches of current word/selection
	    highlightMatches: true,
	    // Whether or not to show the format button on the toolbar
	    showFormatButton: true,
	    // Whether or not to show the comment button on the toolbar
	    showCommentButton: true,
	    // Whether or not to show the uncomment button on the toolbar
	    showUncommentButton: true,
	    // Whether or not to show the showAutoCompleteButton button on the toolbar
	    showAutoCompleteButton: true
	};

};
