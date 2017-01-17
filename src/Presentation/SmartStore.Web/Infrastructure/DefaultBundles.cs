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
			 * TODO: (mc) Delete this once work is finished
			-----------------------------------------------------*/
			bundles.Add(new CustomScriptBundle("~/bundles/image-gallery").Include(
				"~/Content/image-gallery/js/blueimp-gallery.js",
				//"~/Content/image-gallery/js/blueimp-gallery-fullscreen.js",
				"~/Content/image-gallery/js/blueimp-gallery-indicator.js",
				"~/Scripts/smartstore.scrollbutton.js",
				"~/Scripts/jquery.elevatezoom.js",
				"~/Scripts/smartstore.smartgallery.js"));

			bundles.Add(new CustomStyleBundle("~/css/image-gallery").Include(
				"~/Content/smartstore.smartgallery.css",
				"~/Content/image-gallery/css/blueimp-gallery.css",
				"~/Content/image-gallery/css/blueimp-gallery-indicator.css",
				"~/Content/image-gallery/css/blueimp-gallery-custom.css"));


			/* Image Gallery
			 * TODO: (mc) change pathes once work is finished
			-----------------------------------------------------*/
			bundles.Add(new CustomScriptBundle("~/bundles/smart-gallery").Include(
				//"~/Themes/Alpha/Content/smart-gallery/js/blueimp-gallery.js",
				//"~/Themes/Alpha/Content/smart-gallery/js/blueimp-gallery-indicator.js",
				"~/Themes/Alpha/Content/drift/Drift.js",
				"~/Themes/Alpha/Content/photoswipe/photoswipe.js",
				"~/Themes/Alpha/Content/photoswipe/photoswipe-ui-default.js",
				"~/Themes/Alpha/Scripts/smartstore.gallery.js"));


			/* File Upload
			-----------------------------------------------------*/
			bundles.Add(new CustomScriptBundle("~/bundles/fileupload").Include(
					"~/Scripts/jquery-ui/widget.js",
					"~/Content/fileupload/jquery.iframe-transport.js",
					"~/Content/fileupload/jquery.fileupload.js",
					"~/Content/fileupload/jquery.fileupload-single-ui.js"));

			bundles.Add(new CustomStyleBundle("~/css/fileupload").Include(
				"~/Content/fileupload/jquery.fileupload-single-ui.css"));

		}

		public int Priority
		{
			get { return 0; }
		}
	}
}