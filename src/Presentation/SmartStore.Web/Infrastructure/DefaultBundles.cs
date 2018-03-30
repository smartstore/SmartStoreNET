using System;
using System.Web.Optimization;
using BundleTransformer.Core.Bundles;
using BundleTransformer.Core.Orderers;
using SmartStore.Web.Framework.Bundling;

namespace SmartStore.Web.Infrastructure
{
	public class DefaultBundles : IBundleProvider
	{
		public void RegisterBundles(BundleCollection bundles)
		{
			/* Image Gallery
			-----------------------------------------------------*/
			bundles.Add(new CustomScriptBundle("~/bundles/smart-gallery").Include(
				"~/Content/vendors/drift/Drift.js",
				"~/Content/vendors/photoswipe/photoswipe.js",
				"~/Content/vendors/photoswipe/photoswipe-ui-default.js",
				"~/Scripts/smartstore.gallery.js"));

			/* File Upload
			-----------------------------------------------------*/
			bundles.Add(new CustomScriptBundle("~/bundles/fileupload").Include(
				"~/Content/vendors/jquery-ui/widget.js",
				"~/Content/vendors/fileuploader/jquery.iframe-transport.js",
				"~/Content/vendors/fileuploader/jquery.fileupload.js",
				"~/Content/vendors/fileuploader/jquery.fileupload-single-ui.js"));

			/* Summernote
			-----------------------------------------------------*/
			bundles.Add(new CustomScriptBundle("~/bundles/summernote").Include(
				"~/Content/editors/summernote/summernote-bs4.min.js",
				"~/Content/editors/summernote/plugins/smartstore.image.js",
				"~/Content/editors/summernote/plugins/smartstore.link.js",
				"~/Content/editors/summernote/plugins/smartstore.tablestyles.js",
				"~/Content/editors/summernote/plugins/smartstore.cssclass.js",
				"~/Content/editors/summernote/globalinit.js"));

			/* CodeMirror (V 5.3.3)
			-----------------------------------------------------*/
			var cm = "~/Content/editors/CodeMirror/";
			bundles.Add(new CustomScriptBundle("~/bundles/codemirror").Include(
				cm + "codemirror.min.js",
				cm + "addon/fold/xml-fold.min.js",
				cm + "addon/hint/show-hint.min.js",
				cm + "addon/hint/xml-hint.min.js",
				cm + "addon/hint/html-hint.min.js",
				cm + "addon/hint/css-hint.min.js",
				cm + "addon/hint/javascript-hint.min.js",
				cm + "addon/edit/closetag.min.js",
				cm + "addon/edit/closebrackets.min.js",
				cm + "addon/edit/matchtags.min.js",
				cm + "addon/edit/matchbrackets.min.js",
				cm + "addon/mode/multiplex.min.js",
				cm + "addon/mode/overlay.min.js",
				cm + "addon/display/fullscreen.min.js",
				cm + "addon/selection/active-line.min.js",
				cm + "mode/xml/xml.min.js",
				cm + "mode/javascript/javascript.min.js",
				cm + "mode/css/css.min.js",
				cm + "mode/htmlmixed/htmlmixed.min.js",
				cm + "mode/liquid/liquid.js"));

			bundles.Add(new CustomStyleBundle("~/css/codemirror").Include(
				cm + "codemirror.min.css",
				cm + "codemirror.custom.css",
				cm + "addon/hint/show-hint.min.css",
				cm + "addon/display/fullscreen.css",
				cm + "theme/eclipse.min.css",
				cm + "mode/liquid/liquid.css"));

			/* Roxy File Manager
			-----------------------------------------------------*/
			var roxy = "~/Administration/Content/filemanager/";
			var scriptBundle = new CustomScriptBundle("~/bundles/roxyfm").Include(
				roxy + "js/jquery-2.1.1.min.js",
				roxy + "js/jquery-ui-1.10.4.custom.min.js",
				roxy + "js/filetypes.js",
				roxy + "js/custom.js",
				roxy + "js/main.js",
				roxy + "js/utils.js",
				roxy + "js/file.js",
				roxy + "js/directory.js",
				roxy + "js/jquery-dateFormat.min.js");
			scriptBundle.Orderer = new NullOrderer();
			bundles.Add(scriptBundle);
		}

		public int Priority
		{
			get { return 0; }
		}
	}
}