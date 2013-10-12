using System;
using System.Linq;
using System.Web.Mvc;
using Newtonsoft.Json;
using SmartStore.Core.Domain.Discounts;
using SmartStore.Plugin.DiscountRules.HadSpentAmount.Models;
using SmartStore.Services.Discounts;
using SmartStore.Web.Framework.Controllers;

namespace SmartStore.Plugin.DiscountRules.HadSpentAmount.Controllers
{

    public class DiscountRulesHadSpentAmountController : PluginControllerBase
    {
        private readonly IDiscountService _discountService;

        public DiscountRulesHadSpentAmountController(IDiscountService discountService)
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

            // parse settings from ExtraData
            string extraData = null;
            if (discountRequirement != null)
            {
                extraData = discountRequirement.ExtraData;
            }
            var settings = HadSpentAmountDiscountRequirementRule.DeserializeSettings(extraData);

            var model = new RequirementModel();
            model.RequirementId = discountRequirementId.HasValue ? discountRequirementId.Value : 0;
            model.DiscountId = discountId;
            model.SpentAmount = discountRequirement != null ? discountRequirement.SpentAmount : decimal.Zero;
            model.LimitToCurrentBasketSubTotal = settings.LimitToCurrentBasketSubTotal;
            model.BasketSubTotalIncludesDiscounts = settings.BasketSubTotalIncludesDiscounts;

            //add a prefix
            ViewData.TemplateInfo.HtmlFieldPrefix = string.Format("DiscountRulesHadSpentAmount{0}", discountRequirementId.HasValue ? discountRequirementId.Value.ToString() : "0");

            return View("SmartStore.Plugin.DiscountRules.HadSpentAmount.Views.DiscountRulesHadSpentAmount.Configure", model);
        }

        [HttpPost]
        public ActionResult Configure(int discountId, int? discountRequirementId, decimal spentAmount, string settings)
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
                discountRequirement.SpentAmount = spentAmount;
                discountRequirement.ExtraData = settings;

                _discountService.UpdateDiscount(discount);
            }
            else
            {
                //save new rule
                discountRequirement = new DiscountRequirement()
                {
                    DiscountRequirementRuleSystemName = "DiscountRequirement.HadSpentAmount",
                    ExtraData = settings,
                    SpentAmount = spentAmount
                };
                discount.DiscountRequirements.Add(discountRequirement);
                _discountService.UpdateDiscount(discount);
            }
            return Json(new { Result = true, NewRequirementId = discountRequirement.Id }, JsonRequestBehavior.AllowGet);
        }
        
    }
}