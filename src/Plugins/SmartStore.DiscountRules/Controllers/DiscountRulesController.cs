using System;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Core.Domain.Discounts;
using SmartStore.Core.Localization;
using SmartStore.DiscountRules.Models;
using SmartStore.Services.Customers;
using SmartStore.Services.Directory;
using SmartStore.Services.Discounts;
using SmartStore.Services.Stores;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Security;

namespace SmartStore.DiscountRules.Controllers
{

	[AdminAuthorize]
    public class DiscountRulesController : PluginControllerBase
    {
        private readonly IDiscountService _discountService;
        private readonly Lazy<ICustomerService> _customerService;
		private readonly Lazy<ICountryService> _countryService;
		private readonly Lazy<IStoreService> _storeService;

        public DiscountRulesController(
			IDiscountService discountService, 
			Lazy<ICustomerService> customerService,
			Lazy<ICountryService> countryService,
			Lazy<IStoreService> storeService)
        {
            this._discountService = discountService;
            this._customerService = customerService;
			this._countryService = countryService;
			this._storeService = storeService;
        }

		#region Global

		[NonAction]
		private ActionResult HandleGet<TModel>(int discountId, int? discountRequirementId, string prefix, Func<DiscountRequirement, TModel> modelCreator) where TModel : DiscountRuleModelBase
		{
			var discount = _discountService.GetDiscountById(discountId);
			if (discount == null)
				throw new ArgumentException(T("Plugins.SmartStore.DiscountRules.ConfigureDiscountNotLoaded"));

			DiscountRequirement discountRequirement = null;
			if (discountRequirementId.HasValue)
			{
				discountRequirement = discount.DiscountRequirements.Where(dr => dr.Id == discountRequirementId.Value).FirstOrDefault();
				if (discountRequirement == null)
					return Content(T("Plugins.SmartStore.DiscountRules.ConfigureRequirementFailed"));
			}

			var model = modelCreator(discountRequirement);
			model.RequirementId = discountRequirementId.GetValueOrDefault();
			model.DiscountId = discountId;

			//add a prefix
			ViewData.TemplateInfo.HtmlFieldPrefix = string.Format("{0}{1}", prefix, discountRequirementId.GetValueOrDefault(0).ToString());

			return View(model);
		}

		[NonAction]
		private ActionResult HandlePost<TModel>(int discountId, int? discountRequirementId, Action<DiscountRequirement> updater, Func<DiscountRequirement> creator) where TModel : DiscountRuleModelBase
		{
			var discount = _discountService.GetDiscountById(discountId);
			if (discount == null)
				throw new ArgumentException(T("Plugins.SmartStore.DiscountRules.ConfigureDiscountNotLoaded"));

			DiscountRequirement discountRequirement = null;
			if (discountRequirementId.HasValue)
				discountRequirement = discount.DiscountRequirements.Where(dr => dr.Id == discountRequirementId.Value).FirstOrDefault();

			if (discountRequirement != null)
			{
				// update existing rule
				updater(discountRequirement);
				_discountService.UpdateDiscount(discountRequirement.Discount);
			}
			else
			{
				// save new rule
				discountRequirement = creator();
				discount.DiscountRequirements.Add(discountRequirement);
				_discountService.UpdateDiscount(discount);
			}

			return Json(new { Result = true, NewRequirementId = discountRequirement.Id }, JsonRequestBehavior.AllowGet);
		}

		#endregion


		#region Customer Role Is

		public ActionResult CustomerRole(int discountId, int? discountRequirementId)
        {
			return HandleGet<CustomerRoleModel>(discountId, discountRequirementId, "DiscountRulesCustomerRoles", (req) => {
				var model = new CustomerRoleModel();
				// countries
				model.AvailableCustomerRoles.Add(new SelectListItem() { Text = T("Plugins.DiscountRequirement.MustBeAssignedToCustomerRole.ConfigureSelectCustomerRole"), Value = "0" });
				foreach (var cr in _customerService.Value.GetAllCustomerRoles(true))
				{
					model.AvailableCustomerRoles.Add(new SelectListItem() { Text = cr.Name, Value = cr.Id.ToString(), Selected = req != null && cr.Id == req.RestrictedToCustomerRoleId });
					if (req != null && cr.Id == req.RestrictedToCustomerRoleId)
					{
						model.CustomerRoleId = cr.Id;
					}
				}
				return model;
			});
        }

        [HttpPost]
		public ActionResult CustomerRole(int discountId, int? discountRequirementId, int customerRoleId)
        {
			return HandlePost<CustomerRoleModel>(
				discountId,
				discountRequirementId,
				req => req.RestrictedToCustomerRoleId = customerRoleId,
				() => new DiscountRequirement 
				{
					DiscountRequirementRuleSystemName = "DiscountRequirement.MustBeAssignedToCustomerRole",
					RestrictedToCustomerRoleId = customerRoleId
				}
			);
		}

		#endregion


		#region Billing Country Is

		public ActionResult BillingCountry(int discountId, int? discountRequirementId)
		{
			return HandleGet<BillingCountryModel>(discountId, discountRequirementId, "DiscountRulesBillingCountry", (req) =>
			{
				var model = new BillingCountryModel();
				// countries
				model.AvailableCountries.Add(new SelectListItem() { Text = T("Plugins.DiscountRules.BillingCountry.Fields.SelectCountry"), Value = "0" });
				foreach (var c in _countryService.Value.GetAllCountries(true))
				{
					model.AvailableCountries.Add(new SelectListItem() { Text = c.Name, Value = c.Id.ToString(), Selected = req != null && c.Id == req.BillingCountryId });
					if (req != null && c.Id == req.BillingCountryId)
					{
						model.CountryId = c.Id;
					}
				}
				return model;
			});
		}

		[HttpPost]
		public ActionResult BillingCountry(int discountId, int? discountRequirementId, int countryId)
		{
			return HandlePost<CustomerRoleModel>(
				discountId,
				discountRequirementId,
				req => req.BillingCountryId = countryId,
				() => new DiscountRequirement
				{
					DiscountRequirementRuleSystemName = "DiscountRequirement.BillingCountryIs",
					BillingCountryId = countryId,
				}
			);
		}

		#endregion


		#region Shipping Country Is

		public ActionResult ShippingCountry(int discountId, int? discountRequirementId)
		{
			return HandleGet<ShippingCountryModel>(discountId, discountRequirementId, "DiscountRulesShippingCountry", (req) =>
			{
				var model = new ShippingCountryModel();
				//countries
				model.AvailableCountries.Add(new SelectListItem() { Text = T("Plugins.DiscountRules.ShippingCountry.Fields.SelectCountry"), Value = "0" });
				foreach (var c in _countryService.Value.GetAllCountries(true))
				{
					model.AvailableCountries.Add(new SelectListItem() { Text = c.Name, Value = c.Id.ToString(), Selected = req != null && c.Id == req.ShippingCountryId });
					if (req != null && c.Id == req.ShippingCountryId)
					{
						model.CountryId = c.Id;
					}
				}
				return model;
			});
		}

		[HttpPost]
		public ActionResult ShippingCountry(int discountId, int? discountRequirementId, int countryId)
		{
			return HandlePost<ShippingCountryModel>(
				discountId,
				discountRequirementId,
				req => req.ShippingCountryId = countryId,
				() => new DiscountRequirement
				{
					DiscountRequirementRuleSystemName = "DiscountRequirement.ShippingCountryIs",
					ShippingCountryId = countryId
				}
			);
		}

		#endregion


		#region Store

		public ActionResult Store(int discountId, int? discountRequirementId)
		{
			return HandleGet<StoreModel>(discountId, discountRequirementId, "DiscountRulesStore", (req) =>
			{
				var model = new StoreModel();

				if (req != null)
					model.StoreId = req.RestrictedToStoreId ?? 0;

				//stores
				model.AvailableStores.Add(new SelectListItem() { Text = T("Plugins.DiscountRules.Store.Fields.SelectStore").Text, Value = "0" });
				foreach (var s in _storeService.Value.GetAllStores())
					model.AvailableStores.Add(new SelectListItem() { Text = s.Name, Value = s.Id.ToString(), Selected = req != null && s.Id == model.StoreId });
				return model;
			});
		}

		[HttpPost]
		public ActionResult Store(int discountId, int? discountRequirementId, int storeId)
		{
			return HandlePost<StoreModel>(
				discountId,
				discountRequirementId,
				req => req.RestrictedToStoreId = storeId,
				() => new DiscountRequirement
				{
					DiscountRequirementRuleSystemName = "DiscountRequirement.Store",
					RestrictedToStoreId = storeId,
				}
			);
		}

		#endregion


		#region HasOneProduct

		public ActionResult HasOneProduct(int discountId, int? discountRequirementId)
		{
			return HandleGet<HasOneProductModel>(discountId, discountRequirementId, "DiscountRulesHasOneProduct", (req) =>
			{
				var model = new HasOneProductModel();
				model.Products = req != null ? req.RestrictedProductIds : "";
				return model;
			});
		}

		[HttpPost]
		public ActionResult HasOneProduct(int discountId, int? discountRequirementId, string productIds)
		{
			return HandlePost<HasOneProductModel>(
				discountId,
				discountRequirementId,
				req => req.RestrictedProductIds = productIds,
				() => new DiscountRequirement
				{
					DiscountRequirementRuleSystemName = "DiscountRequirement.HasOneProduct",
					RestrictedProductIds = productIds,
				}
			);
		}

		#endregion


		#region HasAllProducts

		public ActionResult HasAllProducts(int discountId, int? discountRequirementId)
		{
			return HandleGet<HasAllProductsModel>(discountId, discountRequirementId, "DiscountRulesHasAllProducts", (req) =>
			{
				var model = new HasAllProductsModel();
				model.Products = req != null ? req.RestrictedProductIds : "";
				return model;
			});
		}

		[HttpPost]
		public ActionResult HasAllProducts(int discountId, int? discountRequirementId, string productIds)
		{
			return HandlePost<HasAllProductsModel>(
				discountId,
				discountRequirementId,
				req => req.RestrictedProductIds = productIds,
				() => new DiscountRequirement
				{
					DiscountRequirementRuleSystemName = "DiscountRequirement.HasAllProducts",
					RestrictedProductIds = productIds,
				}
			);
		}

		#endregion


		#region HadSpentAmount

		public ActionResult HadSpentAmount(int discountId, int? discountRequirementId)
		{
			return HandleGet<HadSpentAmountModel>(discountId, discountRequirementId, "DiscountRulesHadSpentAmount", (req) =>
			{
				// parse settings from ExtraData
				string extraData = null;
				if (req != null)
				{
					extraData = req.ExtraData;
				}
				var settings = HadSpentAmountRule.DeserializeSettings(extraData);

				var model = new HadSpentAmountModel();
				model.SpentAmount = req != null ? req.SpentAmount : decimal.Zero;
				model.LimitToCurrentBasketSubTotal = settings.LimitToCurrentBasketSubTotal;

				return model;
			});
		}

		[HttpPost]
		public ActionResult HadSpentAmount(int discountId, int? discountRequirementId, decimal spentAmount, string settings)
		{
			return HandlePost<HadSpentAmountModel>(
				discountId,
				discountRequirementId,
				req => 
				{
					req.SpentAmount = spentAmount;
					req.ExtraData = settings;
				},
				() => new DiscountRequirement
				{
					DiscountRequirementRuleSystemName = "DiscountRequirement.HadSpentAmount",
					ExtraData = settings,
					SpentAmount = spentAmount
				}
			);
		}

		#endregion
	}
}