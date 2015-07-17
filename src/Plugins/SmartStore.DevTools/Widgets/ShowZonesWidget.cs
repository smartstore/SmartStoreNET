using System.Collections.Generic;
using System.Web;
using System.Web.Routing;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Plugins;
using SmartStore.Services;
using SmartStore.Services.Cms;
using SmartStore.Services.Common;

namespace SmartStore.DevTools.Widgets
{
    [SystemName("Widgets.ShowZones")]
    [FriendlyName("Show Zones")]
    public class ShowZonesWidget : IWidget
    {
        private readonly ICommonServices _services;
        private readonly HttpContextBase _httpContext;
        
        public ShowZonesWidget(ICommonServices services,
            HttpContextBase httpContext,
            IGenericAttributeService genericAttributeService)
        {
            _services = services;
            _httpContext = httpContext;
        }

        public IList<string> GetWidgetZones()
        {
            return new List<string>()
			{
                //Blogs
                "blogpost_page_before_body", "mobile_blogpost_page_before_body",
                "blogpost_page_before_comments", "mobile_blogpost_page_before_comments",
                "blogpost_page_inside_comment", "mobile_blogpost_page_inside_comment",
                "blogpost_page_after_comments", "mobile_blogpost_page_after_comments",
                "bloglist_page_before_posts", "mobile_bloglist_page_before_posts",
                "bloglist_page_inside_post", "mobile_bloglist_page_inside_post",
                "bloglist_page_after_posts", "mobile_bloglist_page_after_posts",

                //Categories 
                "categorydetails_after_breadcrumb",
                "categorydetails_top", "mobile_categorydetails_top",
                "categorydetails_before_subcategories", "mobile_categorydetails_before_subcategories",
                "categorydetails_before_featured_products", "mobile_categorydetails_before_featured_products",
                "categorydetails_after_featured_products",
                "categorydetails_before_product_list", "mobile_categorydetails_before_product_list",
                "categorydetails_bottom", "mobile_categorydetails_bottom",
                "categorydetails_before_filters",

                //Products
                "productbox_add_info",
                "productsbytag_before_product_list",
                "productreviews_page_inside_review",
                "productdetails_overview_top",
                "productdetails_overview_bottom",
                "productsbytag_top", "mobile_productsbytag_top",
                "productsbytag_bottom", "mobile_productsbytag_bottom",
                "productdetails_add_info", "mobile_productdetails_add_info",
                "productdetails_top", "mobile_productdetails_top",
                "productdetails_before_pictures", "mobile_productdetails_before_pictures",
                "productdetails_after_pictures", "mobile_productdetails_after_pictures",
                "productdetails_bottom", "mobile_productdetails_bottom",

                //Navigation
                "productbreadcrumb_before",
                "productbreadcrumb_after",
                "header_menu_before",
                "header_menu_after",
                "megamenu_before_first_item",
                "megamenu_after_last_item",
                "account_dropdown_before",
                "account_dropdown_after",
                "header_links_before", "mobile_header_links_before",
                "header_links_after", "mobile_header_links_after",
                "infoblock_before", "mobile_infoblock_before",
                "infoblock_after", "mobile_infoblock_after",
                "account_navigation_before", "mobile_account_navigation_before",
                "account_navigation_after", "mobile_account_navigation_after",

                //Checkout
                "checkout_progress_before", 
                "checkout_progress_after",
                "checkout_billing_address_top", "mobile_checkout_billing_address_top",
                "checkout_billing_address_middle", "mobile_checkout_billing_address_middle",
                "checkout_billing_address_bottom", "mobile_checkout_billing_address_bottom",
                "checkout_completed_top", "mobile_checkout_completed_top",
                "checkout_completed_bottom", "mobile_checkout_completed_bottom",
                "checkout_confirm_top", "mobile_checkout_confirm_top",
                "checkout_confirm_before_summary", "mobile_checkout_confirm_before_summary",
                "checkout_confirm_bottom", "mobile_checkout_confirm_bottom",
                "checkout_payment_method_top", "mobile_checkout_payment_method_top",
                "checkout_payment_method_bottom", "mobile_checkout_payment_method_bottom",
                "checkout_shipping_address_top", "mobile_checkout_shipping_address_top",
                "checkout_shipping_address_middle", "mobile_checkout_shipping_address_middle",
                "checkout_shipping_address_bottom", "mobile_checkout_shipping_address_bottom",
                "checkout_shipping_method_top", "mobile_checkout_shipping_method_top",
                "checkout_shipping_method_bottom", "mobile_checkout_shipping_method_bottom",

                //PDF
                "invoice_footer_before", 
                "invoice_footer_after", 
                "invoice_header_before", 
                "invoice_header_right", 
                "invoice_header_after", 
                "invoice_top", 
                "invoice_orderinfo_top", 
                "invoice_orderinfotable_after", 
                "invoice_orderinfo_bottom", 
                "invoice_lines_before", 
                "invoice_checkoutattributes_before", 
                "invoice_ordertotals_before", 
                "invoice_ordernotes_before", 
                "invoice_bottom", 
                "pdfcatalog_cover_top", 
                "pdfcatalog_cover_logo_after", 
                "pdfcatalog_cover_storename_after", 
                "pdfcatalog_productdetail_top", 
                "pdfcatalog_productdetail_fulldescription_before", 
                "pdfcatalog_productdetail_picture_before", 
                "pdfcatalog_productdetail_specificationattributes_before", 
                "pdfcatalog_productdetail_bundleditems_before", 
                "pdfcatalog_productdetail_associatedproducts_before", 

                //Manufacturer
                "manufacturerdetails_top", "mobile_manufacturerdetails_top",
                "manufacturerdetails_before_featured_products", "mobile_manufacturerdetails_before_featured_products",
                "manufacturerdetails_after_featured_products",
                "manufacturerdetails_before_filters",
                "manufacturerdetails_before_product_list", "mobile_manufacturerdetails_before_product_list",
                "manufacturerdetails_bottom", "mobile_manufacturerdetails_bottom",

                //Misc
                "compareproducts_row", "mobile_compareproducts_row",
                "productsearch_page_advanced",
                "productsearch_page_basic",
                "productsearch_page_advanced",
                "searchbox", "mobile_searchbox",
                "footer", "mobile_footer",
                "contactus_top", "mobile_contactus_top",
                "contactus_bottom", "mobile_contactus_bottom",
                "productreviews_page_top", "mobile_productreviews_page_top",
                "productreviews_page_inside_review", "mobile_productreviews_page_inside_review",
                "productreviews_page_bottom", "mobile_productreviews_page_bottom",
                "profile_page_info_userdetails", "mobile_profile_page_info_userdetails",
                "profile_page_info_userstats", "mobile_profile_page_info_userstats",
                "notifications", "mobile_notifications",
                "shipmentdetails_top",
                "shipmentdetails_orderinfo_top",
                "shipmentdetails_orderinfotable_after",
                "shipmentdetails_orderinfo_bottom",
                "shipmentdetails_lines_before",
        
                //Global
                "home_page_top", "mobile_home_page_top",
                "home_page_bottom", "mobile_home_page_bottom",
                "main_column_before",
                "main_column_after",
                "left_side_column_before",
                "left_side_column_after_category_navigation",
                "left_side_column_after_manufacturer_navigation",
                "left_side_column_after_tags",
                "left_side_column_after_info",
                "left_side_column_after_poll",
                "left_side_column_after",
                "right_side_column_before",
                "right_side_column_after_recentlyviewed",
                "right_side_column_after_poll",
                "right_side_column_after",
                "body_start_html_tag_after", "mobile_body_start_html_tag_after",
                "content_before", "mobile_content_before",
                "content_after", "mobile_content_after",
                "body_end_html_tag_before", "mobile_body_end_html_tag_after",
                "head_html_tag", "mobile_head_html_tag",
                "admin_navbar_before",
                "admin_navbar_after",
                "admin_breadcrumb",
                "admin_content_before",
                "admin_content_after",
                "admin_menu_before",

                //News
                "newslist_page_before_items", "mobile_newslist_page_before_items",
                "newslist_page_inside_item", "mobile_newslist_page_inside_item",
                "newslist_page_after_items", "mobile_newslist_page_after_items",
                "newsitem_page_before_body", "mobile_newsitem_page_before_body",
                "newsitem_page_before_comments", "mobile_newsitem_page_before_comments",
                "newsitem_page_inside_comment", "mobile_newsitem_page_inside_comment",
                "newsitem_page_after_comments", "mobile_newsitem_page_after_comments",

                //Orders
                "orderdetails_page_top", "mobile_orderdetails_page_top",
                "orderdetails_page_overview", "mobile_orderdetails_page_overview",
                "orderdetails_page_beforeproducts", "mobile_orderdetails_page_beforeproducts",
                "orderdetails_page_afterproducts", "mobile_orderdetails_page_afterproducts",
                "orderdetails_page_aftertotal", "mobile_orderdetails_page_aftertotal",
                "orderdetails_page_bottom", "mobile_orderdetails_page_bottom",

                //Shopping Cart
                "mini_shopping_cart_bottom",
                "order_summary_content_before", "mobile_order_summary_content_before",
                "order_summary_content_totals_left", "mobile_order_summary_content_totals_left",
                "order_summary_content_deals", "mobile_order_summary_content_deals",
                "order_summary_content_totals_bottom", "mobile_order_summary_content_totals_bottom",
                "order_summary_content_after", "mobile_order_summary_content_after"
			};
        }

        public void GetDisplayWidgetRoute(string widgetZone, object model, int storeId, out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            controllerName = "DevTools";
            actionName = "WidgetZone";
            routeValues = new RouteValueDictionary()
            {
                { "Namespaces", "SmartStore.DevTools.Controllers" },
                { "area", "SmartStore.DevTools" },
                { "widgetZone", widgetZone }
            };
        }
    }
}