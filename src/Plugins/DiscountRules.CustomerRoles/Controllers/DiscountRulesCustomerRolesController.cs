using System;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Core.Domain.Discounts;
using SmartStore.Plugin.DiscountRules.CustomerRoles.Models;
using SmartStore.Services.Customers;
using SmartStore.Services.Discounts;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Services.Localization;

namespace SmartStore.Plugin.DiscountRules.CustomerRoles.Controllers
{

    public class DiscountRulesCustomerRolesController : PluginControllerBase
    {
        private readonly IDiscountService _discountService;
        private readonly ICustomerService _customerService;
        private readonly ILocalizationService _localize;

        public DiscountRulesCustomerRolesController(IDiscountService discountService,
            ICustomerService customerService, ILocalizationService localize)
        {
            this._discountService = discountService;
            this._customerService = customerService;
            this._localize = localize;
        }

        public ActionResult Configure(int discountId, int? discountRequirementId)
        {
            var discount = _discountService.GetDiscountById(discountId);
            if (discount == null)
                //throw new ArgumentException("Discount could not be loaded");
                throw new ArgumentException(_localize.GetResource("Plugins.DiscountRequirement.MustBeAssignedToCustomerRole.ConfigureDiscountNotLoaded"));

            DiscountRequirement discountRequirement = null;
            if (discountRequirementId.HasValue)
            {
                discountRequirement = discount.DiscountRequirements.Where(dr => dr.Id == discountRequirementId.Value).FirstOrDefault();
                if (discountRequirement == null)
                    //return Content("Failed to load requirement.");
                    return Content(_localize.GetResource("Plugins.DiscountRequirement.MustBeAssignedToCustomerRole.ConfigureRequirementFailed"));
            }

            var model = new RequirementModel();
            model.RequirementId = discountRequirementId.HasValue ? discountRequirementId.Value : 0;
            model.DiscountId = discountId;
            //countries
            //model.AvailableCustomerRoles.Add(new SelectListItem() { Text = "Select customer role", Value = "0" });
            model.AvailableCustomerRoles.Add(new SelectListItem() { Text = _localize.GetResource("Plugins.DiscountRequirement.MustBeAssignedToCustomerRole.ConfigureSelectCustomerRole"), Value = "0" });
            foreach (var cr in _customerService.GetAllCustomerRoles(true))
            {
                model.AvailableCustomerRoles.Add(new SelectListItem() { Text = cr.Name, Value = cr.Id.ToString(), Selected = discountRequirement != null && cr.Id == discountRequirement.RestrictedToCustomerRoleId });
                if (discountRequirement != null && cr.Id == discountRequirement.RestrictedToCustomerRoleId) 
                {
                    model.CustomerRoleId = cr.Id;
                }
            }
            //add a prefix
            ViewData.TemplateInfo.HtmlFieldPrefix = string.Format("DiscountRulesCustomerRoles{0}", discountRequirementId.HasValue ? discountRequirementId.Value.ToString() : "0");

            return View("SmartStore.Plugin.DiscountRules.CustomerRoles.Views.DiscountRulesCustomerRoles.Configure", model);
        }

        [HttpPost]
        public ActionResult Configure(int discountId, int? discountRequirementId, int customerRoleId)
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
                discountRequirement.RestrictedToCustomerRoleId = customerRoleId;
                _discountService.UpdateDiscount(discount);
            }
            else
            {
                //save new rule
                discountRequirement = new DiscountRequirement()
                {
                    DiscountRequirementRuleSystemName = "DiscountRequirement.MustBeAssignedToCustomerRole",
                    RestrictedToCustomerRoleId = customerRoleId,
                };
                discount.DiscountRequirements.Add(discountRequirement);
                _discountService.UpdateDiscount(discount);
            }
            return Json(new { Result = true, NewRequirementId = discountRequirement.Id }, JsonRequestBehavior.AllowGet);
        }
        
    }
}