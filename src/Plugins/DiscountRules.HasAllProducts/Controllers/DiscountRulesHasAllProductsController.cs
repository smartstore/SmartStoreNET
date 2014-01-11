using System;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Core.Domain.Discounts;
using SmartStore.Plugin.DiscountRules.HasAllProducts.Models;
using SmartStore.Services.Customers;
using SmartStore.Services.Discounts;
using SmartStore.Web.Framework.Controllers;

namespace SmartStore.Plugin.DiscountRules.HasAllProducts.Controllers
{

    public class DiscountRulesHasAllProductsController : PluginControllerBase
    {
        private readonly IDiscountService _discountService;

        public DiscountRulesHasAllProductsController(IDiscountService discountService,
            ICustomerService customerService)
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
            model.RequirementId = discountRequirementId.HasValue ? discountRequirementId.Value : 0;
            model.DiscountId = discountId;
            model.Products = discountRequirement != null ? discountRequirement.RestrictedProductIds : "";

            //add a prefix
            ViewData.TemplateInfo.HtmlFieldPrefix = string.Format("DiscountRulesHasAllProducts{0}", discountRequirementId.HasValue ? discountRequirementId.Value.ToString() : "0");

            return View("SmartStore.Plugin.DiscountRules.HasAllProducts.Views.DiscountRulesHasAllProducts.Configure", model);
        }

        [HttpPost]
        public ActionResult Configure(int discountId, int? discountRequirementId, string productIds)
        {
            var discount = _discountService.GetDiscountById(discountId);
            if (discount == null)
                throw new ArgumentException("Discount could not be loaded");

            DiscountRequirement discountRequirement = null;
            if (discountRequirementId.HasValue)
                discountRequirement = discount.DiscountRequirements.Where(dr => dr.Id == discountRequirementId.Value).FirstOrDefault();

            if (discountRequirement != null)
            {
                //update existing rule
                discountRequirement.RestrictedProductIds = productIds;
                _discountService.UpdateDiscount(discount);
            }
            else
            {
                //save new rule
                discountRequirement = new DiscountRequirement()
                {
                    DiscountRequirementRuleSystemName = "DiscountRequirement.HasAllProducts",
                    RestrictedProductIds = productIds,
                };
                discount.DiscountRequirements.Add(discountRequirement);
                _discountService.UpdateDiscount(discount);
            }
            return Json(new { Result = true, NewRequirementId = discountRequirement.Id }, JsonRequestBehavior.AllowGet);
        }
        
    }
}