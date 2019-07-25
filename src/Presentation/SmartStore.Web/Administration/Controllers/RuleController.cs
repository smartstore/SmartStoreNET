using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Telerik.Web.Mvc;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Rules;
using SmartStore.Services.Security;
using SmartStore.Services.Customers;
using SmartStore.Services.Cart.Rules;
using SmartStore.Web.Framework.Security;

namespace SmartStore.Admin.Controllers
{
    [AdminAuthorize]
    public partial class RuleController : AdminControllerBase
    {
        private readonly IRuleFactory _ruleFactory;
        private readonly IRuleStorage _ruleStorage;
        private readonly ICartRuleProvider _cartRuleProvider;
        private readonly ITargetGroupService _targetGroupService;

        public RuleController(
            IRuleFactory ruleFactory, 
            IRuleStorage ruleStorage,
            ICartRuleProvider cartRuleProvider,
            ITargetGroupService targetGroupService)
        {
            _ruleFactory = ruleFactory;
            _ruleStorage = ruleStorage;
            _cartRuleProvider = cartRuleProvider;
            _targetGroupService = targetGroupService;
        }

        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        public ActionResult List()
        {
            return Content("");
        }

        public ActionResult Edit(int id /* ruleSetId */)
        {
            var group = _targetGroupService.CreateExpressionGroup(id);
            if (group == null)
            {
                return HttpNotFound();
            }

            ViewBag.AvailableDescriptors = _targetGroupService.RuleDescriptors;

            return View(group);
        }
    }
}