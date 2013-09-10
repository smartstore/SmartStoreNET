using System;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Core.Domain.Discounts;
using SmartStore.Plugin.DiscountRules.HasShippingOption.Models;
using SmartStore.Services.Discounts;
using SmartStore.Web.Framework.Controllers;

namespace SmartStore.Plugin.DiscountRules.HasShippingOption.Controllers
{

    public class DiscountRulesHasShippingOptionController : PluginControllerBase
    {
        private readonly IDiscountService _discountService;

		public DiscountRulesHasShippingOptionController(IDiscountService discountService)
        {
            this._discountService = discountService;
        }

        public ActionResult Configure(int discountId, int? discountRequirementId)
        {
            var discount = _discountService.GetDiscountById(discountId);
            if (discount == null)
                throw new ArgumentException("Discount could not be loaded");

            DiscountRequirement discountRequirement = null;
            if (discountRequirementId.HasValue)
            {
                discountRequirement = discount.DiscountRequirements.Where(dr => dr.Id == discountRequirementId.Value).FirstOrDefault();
                if (discountRequirement == null)
                    return Content("Failed to load requirement.");
            }

            var model = new RequirementModel();
            model.RequirementId = discountRequirementId ?? 0;
            model.DiscountId = discountId;

			if (discountRequirement != null)
				model.ShippingOptions = discountRequirement.RestrictedShippingOptions;

            //add a prefix
			ViewData.TemplateInfo.HtmlFieldPrefix = "DiscountRulesHasShippingOption{0}".FormatWith(model.RequirementId);

			return View("SmartStore.Plugin.DiscountRules.HasShippingOption.Views.DiscountRulesHasShippingOption.Configure", model);
        }

        [HttpPost]
		public ActionResult Configure(int discountId, int? discountRequirementId, string shippingOptions)
        {
            var discount = _discountService.GetDiscountById(discountId);
            if (discount == null)
                throw new ArgumentException("Discount could not be loaded");

            DiscountRequirement discountRequirement = null;
            if (discountRequirementId.HasValue)
                discountRequirement = discount.DiscountRequirements.Where(dr => dr.Id == discountRequirementId.Value).FirstOrDefault();

            if (discountRequirement != null)
            {
				discountRequirement.RestrictedShippingOptions = shippingOptions;
				_discountService.UpdateDiscount(discount);
            }
            else
            {
                //save new rule
                discountRequirement = new DiscountRequirement()
                {
					DiscountRequirementRuleSystemName = "DiscountRequirement.HasShippingOption",
					RestrictedShippingOptions = shippingOptions
                };
                discount.DiscountRequirements.Add(discountRequirement);
                _discountService.UpdateDiscount(discount);
            }
            return Json(new { Result = true, NewRequirementId = discountRequirement.Id }, JsonRequestBehavior.AllowGet);
        }
        
    }
}