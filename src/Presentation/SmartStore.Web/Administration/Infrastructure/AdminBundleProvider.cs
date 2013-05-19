using System;
using System.Web;
using System.Web.Optimization;
using SmartStore.Web.Framework.Mvc.Bundles;
using SmartStore.Web.Framework.Less;

namespace SmartStore.Admin.Infrastructure
{
    // Weitere Informationen zu Bundling finden Sie unter "http://go.microsoft.com/fwlink/?LinkId=254725"
    /// <summary>
    /// <remarks>codehint: sm-add</remarks>
    /// </summary>
    public class AdminBundleProvider : IBundleProvider
    {
        public void RegisterBundles(BundleCollection bundles)
        {
            //Bundle bundle;
            //LessMinify lessMinify = new LessMinify();
            
            bundles.Add(new ScriptBundle("~/bundles/admin/app").Include(
                "~/Administration/Scripts/admin.common.js",
                "~/Administration/Scripts/admin.globalinit.js"));
            
            //bundle = new StyleBundle("~/Content/admin/theme").Include(
            //    "~/Administration/Content/theme.less");
            //bundle.Transforms.Add(lessMinify);
            //bundles.Add(bundle);

            bundles.Add(new ScriptBundle("~/bundles/admin/tiny-mce-js").Include(
                "~/Content/editors/tinymce/tiny_mce_popup.js",
                "~/Content/editors/tinymce/utils/mctabs.js",    
                "~/Content/editors/tinymce/utils/form_utils.js",
                "~/Content/editors/tinymce/utils/validate.js",
                "~/Content/editors/tinymce/utils/editable_selects.js",
                "~/Content/editors/tinymce/plugins/netadvimage/js/jquery.js",
                "~/Content/editors/tinymce/plugins/netadvimage/js/jquery-ui.js",
                "~/Content/editors/tinymce/plugins/netadvimage/js/jquery.layout.js",
                "~/Content/editors/tinymce/plugins/netadvimage/js/dialog.js",
                "~/Content/editors/tinymce/plugins/netadvimage/js/fileuploader.js"));

            // shared widgets, components etc.
            bundles.Add(new StyleBundle("~/Content/tiny-mce-css").Include(
                "~/Content/editors/tinymce/plugins/netadvimage/css/dialog.css",
                "~/Content/editors/tinymce/plugins/netadvimage/css/fileuploader.css"));

        }

        public int Priority
        {
            get { return 1; }
        }
    }
}