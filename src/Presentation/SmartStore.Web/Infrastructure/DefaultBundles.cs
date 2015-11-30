using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Optimization;
using BundleTransformer.Core.Bundles;
using SmartStore.Web.Framework.Mvc.Bundles;

namespace SmartStore.Web.Infrastructure
{
	public class DefaultBundles : IBundleProvider
	{
		public void RegisterBundles(BundleCollection bundles)
		{
			/* Image Gallery
			-----------------------------------------------------*/
			bundles.Add(new CustomScriptBundle("~/bundles/image-gallery").Include(
				"~/Content/image-gallery/js/blueimp-gallery.js",
				//"~/Content/image-gallery/js/blueimp-gallery-fullscreen.js",
				"~/Content/image-gallery/js/blueimp-gallery-indicator.js",
				"~/Scripts/jquery.elevatezoom.js",
				"~/Scripts/smartstore.smartgallery.js"));

			bundles.Add(new CustomStyleBundle("~/css/image-gallery").Include(
				"~/Content/smartstore.smartgallery.css",
				"~/Content/image-gallery/css/blueimp-gallery.css",
				"~/Content/image-gallery/css/blueimp-gallery-indicator.css",
				"~/Content/image-gallery/css/blueimp-gallery-custom.css"));


			/* sequence js
			-----------------------------------------------------*/
			bundles.Add(new CustomScriptBundle("~/bundles/sequencejs").Include(
				"~/Scripts/jquery.backgroundpos.js",
				"~/Scripts/jquery.sequence.js",
				"~/Scripts/jquery.sequence.custom.js"));


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