using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using SmartStore.ComponentModel;
using SmartStore.Core;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Logging;
using SmartStore.GoogleAnalytics.Models;
using SmartStore.Services.Catalog;
using SmartStore.Services.Configuration;
using SmartStore.Services.Orders;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Security;
using SmartStore.Web.Framework.Settings;
using SmartStore.Core.Localization;

namespace SmartStore.GoogleAnalytics.Controllers
{
	public class WidgetsGoogleAnalyticsController : SmartController
    {
        private readonly IWorkContext _workContext;
		private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;
        private readonly IOrderService _orderService;
        private readonly ICategoryService _categoryService;

        public WidgetsGoogleAnalyticsController(
			IWorkContext workContext,
			IStoreContext storeContext,
			ISettingService settingService,
			IOrderService orderService,
            ICategoryService categoryService)
        {
            _workContext = workContext;
			_storeContext = storeContext;
            _settingService = settingService;
            _orderService = orderService;
            _categoryService = categoryService;

			T = NullLocalizer.Instance;
		}

		public Localizer T { get; set; }

		[AdminAuthorize, ChildActionOnly, LoadSetting]
        public ActionResult Configure(GoogleAnalyticsSettings settings)
        {
            var model = new ConfigurationModel();
			MiniMapper.Map(settings, model);
            
            model.ZoneId = settings.WidgetZone;
            model.AvailableZones.Add(new SelectListItem { Text = "<head> HTML tag", Value = "head_html_tag"});
            model.AvailableZones.Add(new SelectListItem { Text = "Before <body> end HTML tag", Value = "body_end_html_tag_before" });

            return View(model);
        }

        [HttpPost, AdminAuthorize, ChildActionOnly, ValidateInput(false)]
        public ActionResult Configure(ConfigurationModel model, FormCollection form)
        {
			var storeDependingSettingHelper = new StoreDependingSettingHelper(ViewData);
			var storeScope = this.GetActiveStoreScopeConfiguration(Services.StoreService, Services.WorkContext);
			var settings = Services.Settings.LoadSetting<GoogleAnalyticsSettings>(storeScope);

			ModelState.Clear();

			MiniMapper.Map(model, settings);
			settings.WidgetZone = model.ZoneId;

			using (Services.Settings.BeginScope())
			{
				storeDependingSettingHelper.UpdateSettings(settings, form, storeScope, Services.Settings);
			}

			using (Services.Settings.BeginScope())
			{
				_settingService.SaveSetting(settings, x => x.WidgetZone, 0, false);
			}

			return RedirectToConfiguration("SmartStore.GoogleAnalytics");
        }

        [ChildActionOnly]
        public ActionResult PublicInfo(string widgetZone)
        {
            string globalScript = "";
            var routeData = ((System.Web.UI.Page)this.HttpContext.CurrentHandler).RouteData;

            try
            {			
				// Special case, if we are in last step of checkout, we can use order total for conversion value
				if (routeData.Values["controller"].ToString().Equals("checkout", StringComparison.InvariantCultureIgnoreCase) &&
                    routeData.Values["action"].ToString().Equals("completed", StringComparison.InvariantCultureIgnoreCase))
                {
                    var lastOrder = GetLastOrder();
                    globalScript += GetEcommerceScript(lastOrder);
                }
                else
                {
                    globalScript += GetTrackingScript();
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error creating scripts for google ecommerce tracking");
            }
            return Content(globalScript);
        }

        private Order GetLastOrder()
        {
			var order = _orderService.SearchOrders(_storeContext.CurrentStore.Id, _workContext.CurrentCustomer.Id,
				null, null, null, null, null, null, null, null, 0, 1).FirstOrDefault();
			return order;
        }

		private string GetOptOutCookieScript()
		{
			var settings = _settingService.LoadSetting<GoogleAnalyticsSettings>(_storeContext.CurrentStore.Id);
			var script = @"
				var gaProperty = '{GOOGLEID}'; 
				var disableStr = 'ga-disable-' + gaProperty; 
				if (document.cookie.indexOf(disableStr + '=true') > -1) { 
					window[disableStr] = true;
				} 
				function gaOptout() { 
					document.cookie = disableStr + '=true; expires=Thu, 31 Dec 2099 23:59:59 UTC; path=/'; 
					window[disableStr] = true; 
					alert('{NOTIFICATION}'); 
				} 
			";

			script = script + "\n";
			script = script.Replace("{GOOGLEID}", settings.GoogleId);
			script = script.Replace("{NOTIFICATION}", T("Plugins.Widgets.GoogleAnalytics.OptOutNotification").JsText.ToHtmlString());

			return script;
		}
		
		private string GetTrackingScript()
        {
			var settings = _settingService.LoadSetting<GoogleAnalyticsSettings>(_storeContext.CurrentStore.Id);
            var script = "";
            script = settings.TrackingScript + "\n";
            script = script.Replace("{GOOGLEID}", settings.GoogleId);
            script = script.Replace("{ECOMMERCE}", "");
			script = script.Replace("{OPTOUTCOOKIE}", GetOptOutCookieScript());

			return script;
        }
        
        private string GetEcommerceScript(Order order)
        {
			var settings = _settingService.LoadSetting<GoogleAnalyticsSettings>(_storeContext.CurrentStore.Id);
            var usCulture = new CultureInfo("en-US");
            var script = "";
			var ecScript = "";

			script = settings.TrackingScript + "\n";
			script = script.Replace("{GOOGLEID}", settings.GoogleId);
			script = script.Replace("{OPTOUTCOOKIE}", GetOptOutCookieScript());

			if (order != null)
            {
				var site = _storeContext.CurrentStore.Url
					.EmptyNull()
					.Replace("http://", "")
					.Replace("https://", "")
					.Replace("/", "");

				ecScript = settings.EcommerceScript + "\n";
                ecScript = ecScript.Replace("{GOOGLEID}", settings.GoogleId);
                ecScript = ecScript.Replace("{ORDERID}", order.GetOrderNumber());
				ecScript = ecScript.Replace("{SITE}", FixIllegalJavaScriptChars(site));
                ecScript = ecScript.Replace("{TOTAL}", order.OrderTotal.ToString("0.00", usCulture));
                ecScript = ecScript.Replace("{TAX}", order.OrderTax.ToString("0.00", usCulture));
                ecScript = ecScript.Replace("{SHIP}", order.OrderShippingInclTax.ToString("0.00", usCulture));
                ecScript = ecScript.Replace("{CITY}", order.BillingAddress == null ? "" : FixIllegalJavaScriptChars(order.BillingAddress.City));
                ecScript = ecScript.Replace("{STATEPROVINCE}", order.BillingAddress == null || order.BillingAddress.StateProvince == null 
					? "" 
					: FixIllegalJavaScriptChars(order.BillingAddress.StateProvince.Name));
                ecScript = ecScript.Replace("{COUNTRY}", order.BillingAddress == null || order.BillingAddress.Country == null 
					? ""
					: FixIllegalJavaScriptChars(order.BillingAddress.Country.Name));
                ecScript = ecScript.Replace("{CURRENCY}", order.CustomerCurrencyCode);

                var sb = new StringBuilder();
                foreach (var item in order.OrderItems)
                {
                    var ecDetailScript = settings.EcommerceDetailScript;
                    var defaultProductCategory = _categoryService.GetProductCategoriesByProductId(item.ProductId).FirstOrDefault();
					var categoryName = defaultProductCategory != null
						? defaultProductCategory.Category.Name
						: "";

					// The SKU code is a required parameter for every item that is added to the transaction.
					item.Product.MergeWithCombination(item.AttributesXml);

					ecDetailScript = ecDetailScript.Replace("{ORDERID}", order.GetOrderNumber());
                    ecDetailScript = ecDetailScript.Replace("{PRODUCTSKU}", FixIllegalJavaScriptChars(item.Product.Sku));
                    ecDetailScript = ecDetailScript.Replace("{PRODUCTNAME}", FixIllegalJavaScriptChars(item.Product.Name));
                    ecDetailScript = ecDetailScript.Replace("{CATEGORYNAME}", FixIllegalJavaScriptChars(categoryName));
                    ecDetailScript = ecDetailScript.Replace("{UNITPRICE}", item.UnitPriceInclTax.ToString("0.00", usCulture));
                    ecDetailScript = ecDetailScript.Replace("{QUANTITY}", item.Quantity.ToString());

                    sb.AppendLine(ecDetailScript);
                }

                ecScript = ecScript.Replace("{DETAILS}", sb.ToString());
                script = script.Replace("{ECOMMERCE}", ecScript);
            }

            return script;
        }

        private string FixIllegalJavaScriptChars(string text)
        {
            if (String.IsNullOrEmpty(text))
                return text;

            //replace ' with \' (http://stackoverflow.com/questions/4292761/need-to-url-encode-labels-when-tracking-events-with-google-analytics)
            text = text.Replace("'", "\\'");
            return text;
        }
    }
}