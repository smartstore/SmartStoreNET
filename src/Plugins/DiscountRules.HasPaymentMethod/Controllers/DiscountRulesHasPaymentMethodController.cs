using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Core.Domain.Discounts;
using SmartStore.Plugin.DiscountRules.HasPaymentMethod.Models;
using SmartStore.Services.Discounts;
using SmartStore.Services.Localization;
using SmartStore.Services.Payments;
using SmartStore.Web.Framework.Controllers;

namespace SmartStore.Plugin.DiscountRules.HasPaymentMethod.Controllers
{

    public class DiscountRulesHasPaymentMethodController : PluginControllerBase
    {
        private readonly IDiscountService _discountService;
		private readonly IPaymentService _paymentService;
		private readonly ILocalizationService _localizationService;

		public DiscountRulesHasPaymentMethodController(IDiscountService discountService,
			IPaymentService paymentService,
			ILocalizationService localizationService)
        {
            this._discountService = discountService;
			this._paymentService = paymentService;
			this._localizationService = localizationService;
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
			model.AvailablePaymentMethods = new List<SelectListItem>();

			var paymentMethods = _paymentService.LoadActivePaymentMethods().ToList();
			List<string> restrictedPaymentMethods = null;

			if (discountRequirement != null && discountRequirement.RestrictedPaymentMethods != null)
			{
				model.PaymentMethods = discountRequirement.RestrictedPaymentMethods;

				restrictedPaymentMethods = discountRequirement.RestrictedPaymentMethods
					.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList();
			}

			foreach (var method in paymentMethods)
			{
				var item = new SelectListItem()
				{
					Text = method.PluginDescriptor.GetLocalizedFriendlyName(_localizationService),
					Value = method.PluginDescriptor.SystemName,
					Selected = (restrictedPaymentMethods != null && restrictedPaymentMethods.Contains(method.PluginDescriptor.SystemName))
				};

				model.AvailablePaymentMethods.Add(item);
			}

            //add a prefix
			ViewData.TemplateInfo.HtmlFieldPrefix = "DiscountRulesHasPaymentMethod{0}".FormatWith(model.RequirementId);

			return View("SmartStore.Plugin.DiscountRules.HasPaymentMethod.Views.DiscountRulesHasPaymentMethod.Configure", model);
        }

        [HttpPost]
		public ActionResult Configure(int discountId, int? discountRequirementId, string paymentMethods)
        {
            var discount = _discountService.GetDiscountById(discountId);
            if (discount == null)
                throw new ArgumentException("Discount could not be loaded");

            DiscountRequirement discountRequirement = null;
            if (discountRequirementId.HasValue)
                discountRequirement = discount.DiscountRequirements.Where(dr => dr.Id == discountRequirementId.Value).FirstOrDefault();

            if (discountRequirement != null)
            {
				discountRequirement.RestrictedPaymentMethods = paymentMethods;
				_discountService.UpdateDiscount(discount);
            }
            else
            {
                //save new rule
                discountRequirement = new DiscountRequirement()
                {
					DiscountRequirementRuleSystemName = "DiscountRequirement.HasPaymentMethod",
					RestrictedPaymentMethods = paymentMethods
                };
                discount.DiscountRequirements.Add(discountRequirement);
                _discountService.UpdateDiscount(discount);
            }
            return Json(new { Result = true, NewRequirementId = discountRequirement.Id }, JsonRequestBehavior.AllowGet);
        }
        
    }
}