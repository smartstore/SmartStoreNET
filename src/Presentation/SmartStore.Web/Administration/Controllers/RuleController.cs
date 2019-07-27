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
using SmartStore.Rules.Domain;
using System.Globalization;

namespace SmartStore.Admin.Controllers
{
    [AdminAuthorize]
    public partial class RuleController : AdminControllerBase
    {
        private readonly IRuleFactory _ruleFactory;
        private readonly IRuleStorage _ruleStorage;
        private readonly ICartRuleProvider _cartRuleProvider;
        private readonly ITargetGroupService _targetGroupService;
        private readonly Func<RuleScope, IRuleProvider> _ruleProvider;

        public RuleController(
            IRuleFactory ruleFactory, 
            IRuleStorage ruleStorage,
            ICartRuleProvider cartRuleProvider,
            ITargetGroupService targetGroupService,
            Func<RuleScope, IRuleProvider> ruleProvider)
        {
            _ruleFactory = ruleFactory;
            _ruleStorage = ruleStorage;
            _cartRuleProvider = cartRuleProvider;
            _targetGroupService = targetGroupService;
            _ruleProvider = ruleProvider;
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

            return View(model);
        }

        [HttpPost]
        public ActionResult AddRule(int ruleSetId, RuleScope scope, string ruleType)
        {
            var provider = _ruleProvider(scope);
            var descriptor = provider.RuleDescriptors.FindDescriptor(ruleType);

            var rule = new RuleEntity
            {
                RuleSetId = ruleSetId,
                RuleType = ruleType,
                Operator = descriptor.Operators.First().Operator
            };

            _ruleStorage.InsertRule(rule);

            var expression = provider.VisitRule(rule);

            return PartialView("_Rule", expression);
        }

        [HttpPost]
        public ActionResult DeleteRule(int ruleId)
        {
            var rule = _ruleStorage.GetRuleById(ruleId, true);
            if (rule == null)
            {
                // TODO: message/notify
                return Json(new { Success = false, Message = "" });
            }

            _ruleStorage.DeleteRule(rule);

            return Json(new { Success = true });
        }

        [HttpPost]
        public ActionResult AddGroup(int ruleSetId, RuleScope scope)
        {
            var provider = _ruleProvider(scope);

            var group = new RuleSetEntity
            {
                IsActive = true,
                IsSubGroup = true,
                Scope = scope
            };
            _ruleStorage.InsertRuleSet(group);

            var groupRefRule = new RuleEntity
            {
                RuleSetId = ruleSetId,
                RuleType = "Group",
                Operator = RuleOperator.IsEqualTo,
                Value = group.Id.ToString()
            };

            _ruleStorage.InsertRule(groupRefRule);

            var expression = provider.VisitRuleSet(group);

            return PartialView("_RuleSet", expression);
        }

        [HttpPost]
        public ActionResult DeleteGroup(int refRuleId)
        {
            var refRule = _ruleStorage.GetRuleById(refRuleId, true);
            var ruleSetId = refRule.Value.ToInt();

            var group = _ruleStorage.GetRuleSetById(ruleSetId, true, false);
            if (group == null)
            {
                // TODO: message/notify
                return Json(new { Success = false, Message = "" });
            }

            _ruleStorage.DeleteRule(refRule);
            _ruleStorage.DeleteRuleSet(group);

            return Json(new { Success = true });
        }

        public ActionResult Execute(int ruleSetId)
        {
            var result = _targetGroupService.ProcessFilter(new[] { ruleSetId }, 0, 500);

            return Content($"{result.TotalCount} Kunden entsprechen dem Filter.");
        }
    }
}