using System;
using System.Web;
using System.Web.Optimization;
using SmartStore.Web.Framework.Mvc.Bundles;

namespace SmartStore.Web.Infrastructure
{
    // Weitere Informationen zu Bundling finden Sie unter "http://go.microsoft.com/fwlink/?LinkId=254725"
    /// <summary>
    /// <remarks>codehint: sm-add</remarks>
    /// </summary>
    public class DefaultBundleProvider : IBundleProvider
    {
        public void RegisterBundles(BundleCollection bundles)
        {
            bundles.UseCdn = true;

            RegisterStylesheets(bundles);
            RegisterScripts(bundles);
        }

        private void RegisterStylesheets(BundleCollection bundles)
        {
            Bundle bundle;
            
            // jquery ui
            var jqueryCssRootPath = "~/Content/jquery-ui/base/";
            bundles.Add(new StyleBundle("~/Content/jquery-ui/base/css").Include(
                jqueryCssRootPath + "jquery.ui.core.css",
                jqueryCssRootPath + "jquery.ui.resizable.css",
                jqueryCssRootPath + "jquery.ui.selectable.css",
                jqueryCssRootPath + "jquery.ui.accordion.css",
                jqueryCssRootPath + "jquery.ui.autocomplete.css",
                jqueryCssRootPath + "jquery.ui.button.css",
                jqueryCssRootPath + "jquery.ui.dialog.css",
                jqueryCssRootPath + "jquery.ui.slider.css",
                jqueryCssRootPath + "jquery.ui.tabs.css",
                jqueryCssRootPath + "jquery.ui.datepicker.css",
                jqueryCssRootPath + "jquery.ui.progressbar.css",
                jqueryCssRootPath + "jquery.ui.theme.css"));

            // telerik common
            var telerikCssRootPath = "~/Content/2012.2.607/";
            bundle = new StyleBundle("~/Content/telerik/common/css");
            bundle.Include(
                telerikCssRootPath + "telerik.common.css",
                telerikCssRootPath + "telerik.rtl.css");
            bundles.Add(bundle);

            // shared widgets, components etc.
            bundles.Add(new StyleBundle("~/Content/shared-ui").Include(
                "~/Content/jquery.pnotify.default.css",
                "~/Content/jquery.pnotify.default.icons.css"));

            // image gallery
            bundles.Add(new StyleBundle("~/Content/image-gallery").Include(
                "~/Content/smartstore.smartgallery.css"));

            // x-editable (with bootstrap adapter)
            bundles.Add(new StyleBundle("~/Content/x-editable").Include(
                "~/Content/x-editable/bootstrap-editable.css"));

        }

        private void RegisterScripts(BundleCollection bundles)
        {
            // modernizr
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                "~/Scripts/modernizr-{version}.js"));

            // jquery
            var jQueryCdnPath = "//ajax.googleapis.com/ajax/libs/jquery/1.8.3/jquery.min.js";
            bundles.Add(new ScriptBundle("~/bundles/jquery", jQueryCdnPath).Include(
                "~/Scripts/jquery-1.8.3.js"));

            // jquery ui
            bundles.Add(new ScriptBundle("~/bundles/jquery-ui-core").Include(
                "~/Scripts/jquery-ui/jquery.ui.core.js",
                "~/Scripts/jquery-ui/jquery.ui.widget.js",
                "~/Scripts/jquery-ui/jquery.effects.core.js",
                "~/Scripts/jquery-ui/jquery.effects.transfer.js",
                "~/Scripts/jquery-ui/jquery.effects.shake.js",
                "~/Scripts/jquery-ui/jquery.ui.position.js"));

            // jquery ui widgets
            bundles.Add(new ScriptBundle("~/bundles/jquery-ui").Include(
                "~/Scripts/jquery-ui/jquery.ui.autocomplete.js"));

            // jquery validation
            bundles.Add(new ScriptBundle("~/bundles/jquery-val").Include(
                "~/Scripts/jquery.unobtrusive-ajax.js",
                "~/Scripts/jquery.validate.js",
                "~/Scripts/jquery.validate.unobtrusive.js"));

            // system (underscore, core helpers, utils etc.)
            bundles.Add(new ScriptBundle("~/bundles/system").Include(
                "~/Scripts/smartstore.system.js",
                "~/Scripts/underscore.js",
                "~/Scripts/underscore.string.js",
                "~/Scripts/underscore.mixins.js",
                "~/Scripts/smartstore.jquery.utils.js",
                "~/Scripts/jquery.ba-outside-events.js",
                "~/Scripts/jquery.preload.js",
                "~/Scripts/jquery.transit.js",
                "~/Scripts/jquery.menu-aim.js",
                "~/Scripts/smartstore.doAjax.js",
                "~/Scripts/jquery.addeasing.js",
                "~/Scripts/smartstore.eventbroker.js",
                "~/Scripts/smartstore.hacks.js"));

            // twitter bootstrap
            //var bootstrapCdnPath = "//netdna.bootstrapcdn.com/twitter-bootstrap/2.1.1/js/bootstrap.min.js";
            var bootstrapJsRoot = "~/Content/bootstrap/js/";
            bundles.Add(new ScriptBundle("~/bundles/bootstrap"/*, bootstrapCdnPath*/).Include(
                bootstrapJsRoot + "bootstrap-transition.js",
                bootstrapJsRoot + "bootstrap-alert.js",
                bootstrapJsRoot + "bootstrap-button.js",
                bootstrapJsRoot + "bootstrap-carousel.js",
                bootstrapJsRoot + "bootstrap-collapse.js",
                bootstrapJsRoot + "bootstrap-dropdown.js",
                bootstrapJsRoot + "bootstrap-modal.js",
                bootstrapJsRoot + "bootstrap-tooltip.js",
                bootstrapJsRoot + "bootstrap-popover.js",
                bootstrapJsRoot + "bootstrap-scrollspy.js",
                bootstrapJsRoot + "bootstrap-tab.js",
                bootstrapJsRoot + "bootstrap-typeahead.js",
                bootstrapJsRoot + "bootstrap-affix.js"));

            // bootstrap custom/3rdparty components
            bundles.Add(new ScriptBundle("~/bundles/bootstrap-custom").Include(
                bootstrapJsRoot + "custom/bootstrap-datepicker.js"));

            // shared widgets, components etc.
            bundles.Add(new ScriptBundle("~/bundles/shared-ui").Include(
                "~/Scripts/smartstore.placeholder.js",
                "~/Scripts/select2.js",
                "~/Scripts/smartstore.selectwrapper.js",
                "~/Scripts/smartstore.throbber.js",
                "~/Scripts/smartstore.navbar.js",
                "~/Scripts/smartstore.thumbzoomer.js",
                "~/Scripts/smartstore.column-equalizer.js",
                "~/Scripts/smartstore.shrinkmenu.js",
                "~/Scripts/smartstore.scrollbutton.js",
                "~/Scripts/smartstore.tabselector.js",
                "~/Scripts/jquery.pnotify.js",
                "~/Scripts/jquery.scrollTo.js",
                "~/Scripts/jquery.serialScroll.js"));

            // content slider
            bundles.Add(new ScriptBundle("~/bundles/slider").Include(
                "~/Scripts/jquery.backgroundpos.js",
                "~/Scripts/jquery.sequence.js",
                "~/Scripts/jquery.sequence.custom.js"));

            // image gallery
            bundles.Add(new ScriptBundle("~/bundles/image-gallery").Include(
                "~/Scripts/jquery.elevatezoom.js",
                "~/Scripts/smartstore.smartgallery.js"));

            // x-editable (with bootstrap adapter)
            bundles.Add(new ScriptBundle("~/bundles/x-editable").Include(
                "~/Content/x-editable/bootstrap-editable.js"));

            // colorpicker
            bundles.Add(new ScriptBundle("~/bundles/colorbox").Include(
                bootstrapJsRoot + "custom/bootstrap-colorpicker.js",
                bootstrapJsRoot + "custom/bootstrap-colorpicker-globalinit.js"));

            // file upload
            bundles.Add(new ScriptBundle("~/bundles/fileupload-single").Include(
                "~/Content/fileupload/jquery.iframe-transport.js",
                "~/Content/fileupload/jquery.fileupload.js",
                "~/Content/fileupload/jquery.fileupload-single-ui.js"));

            // public shop
            bundles.Add(new ScriptBundle("~/bundles/shop").Include(
                "~/Scripts/public.common.js",
                "~/Scripts/public.ajaxcart.js",
                "~/Scripts/public.shopbar.js",
                "~/Scripts/public.product-list-scroller.js",
				"~/Scripts/public.product-filter.js"));
        }

        public int Priority
        {
            get { return 0; }
        }
    }
}