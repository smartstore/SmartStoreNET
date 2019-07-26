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
using SmartStore.Admin.Models.Rules;
using SmartStore.ComponentModel;

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
            var entity = _ruleStorage.GetRuleSetById(id, false, true);
            if (entity == null || entity.IsSubGroup)
            {
                return HttpNotFound();
            }

            var model = new RuleSetModel();
            MiniMapper.Map(entity, model);

            model.ExpressionGroup = _ruleFactory.CreateExpressionGroup(entity, _targetGroupService);
            model.AvailableDescriptors = _targetGroupService.RuleDescriptors;
            ViewBag.RootModel = model;

            return View(model);
        }
    }
}