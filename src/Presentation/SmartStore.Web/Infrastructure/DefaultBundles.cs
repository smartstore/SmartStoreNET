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

			bundles.Add(new CustomStyleBundle("~/css/fileupload").Include(
				"~/Content/fileupload/jquery.fileupload-single-ui.css"));

		}

		public int Priority
		{
			get { return 0; }
		}
	}
}