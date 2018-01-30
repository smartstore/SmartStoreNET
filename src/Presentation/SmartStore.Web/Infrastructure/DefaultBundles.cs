using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Optimization;
using BundleTransformer.Core.Bundles;
using SmartStore.Web.Framework.Bundling;

namespace SmartStore.Web.Infrastructure
{
	public class DefaultBundles : IBundleProvider
	{
		public void RegisterBundles(BundleCollection bundles)
		{
			/* Image Gallery
			 * TODO: (mc) change pathes once work is finished
			-----------------------------------------------------*/
			bundles.Add(new CustomScriptBundle("~/bundles/smart-gallery").Include(
				"~/Themes/Flex/Content/vendors/drift/Drift.js",
				"~/Themes/Flex/Content/vendors/photoswipe/photoswipe.js",
				"~/Themes/Flex/Content/vendors/photoswipe/photoswipe-ui-default.js",
				"~/Themes/Flex/Scripts/smartstore.gallery.js"));

			/* File Upload
			-----------------------------------------------------*/
			bundles.Add(new CustomScriptBundle("~/bundles/fileupload").Include(
				"~/Scripts/jquery-ui/widget.js",
				"~/Content/fileupload/jquery.iframe-transport.js",
				"~/Content/fileupload/jquery.fileupload.js",
				"~/Content/fileupload/jquery.fileupload-single-ui.js"));


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
		}

		public int Priority
		{
			get { return 0; }
		}
	}
}