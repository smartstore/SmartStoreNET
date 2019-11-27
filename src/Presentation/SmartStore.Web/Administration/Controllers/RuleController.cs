using System;
using System.Linq;
using System.Web.Mvc;
using Autofac;
using SmartStore.Admin.Models.Rules;
using SmartStore.ComponentModel;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Security;
using SmartStore.Rules;
using SmartStore.Rules.Domain;
using SmartStore.Services.Cart.Rules;
using SmartStore.Services.Customers;
using SmartStore.Services.Localization;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Filters;
using SmartStore.Web.Framework.Security;
using Telerik.Web.Mvc;

namespace SmartStore.Admin.Controllers
{
    [AdminAuthorize]
    public partial class RuleController : AdminControllerBase
    {
        private readonly IRuleFactory _ruleFactory;
        private readonly IRuleStorage _ruleStorage;
        private readonly ITargetGroupService _targetGroupService;
        private readonly Func<RuleScope, IRuleProvider> _ruleProvider;
        private readonly AdminAreaSettings _adminAreaSettings;

        public RuleController(
            IRuleFactory ruleFactory, 
            IRuleStorage ruleStorage,
            ITargetGroupService targetGroupService,
            Func<RuleScope, IRuleProvider> ruleProvider,
            AdminAreaSettings adminAreaSettings)
        {
            _ruleFactory = ruleFactory;
            _ruleStorage = ruleStorage;
            _targetGroupService = targetGroupService;
            _ruleProvider = ruleProvider;
            _adminAreaSettings = adminAreaSettings;
        }

        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        [Permission(Permissions.System.Rule.Read)]
        public ActionResult List()
        {
            var model = new RuleSetListModel
            {
                GridPageSize = _adminAreaSettings.GridPageSize
            };

            foreach (var s in Services.StoreService.GetAllStores())
            {
                model.AvailableStores.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString() });
            }

            return View(model);
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.System.Rule.Read)]
        public ActionResult List(GridCommand command)
        {
            var gridModel = new GridModel<RuleSetModel>();
            var ruleSets = _ruleStorage.GetAllRuleSets(false, false, null, command.Page - 1, command.PageSize, false, true);

            gridModel.Data = ruleSets.Select(x =>
            {
                var rsModel = MiniMapper.Map<RuleSetEntity, RuleSetModel>(x);
                rsModel.ScopeName = x.Scope.GetLocalizedEnum(Services.Localization, Services.WorkContext);
                return rsModel;
            });

            gridModel.Total = ruleSets.TotalCount;

            return new JsonResult
            {
                MaxJsonLength = int.MaxValue,
                Data = gridModel
            };
        }

        [Permission(Permissions.System.Rule.Create)]
        public ActionResult Create()
        {
            var model = new RuleSetModel();

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [ValidateInput(false)]
        [Permission(Permissions.System.Rule.Create)]
        public ActionResult Create(RuleSetModel model, bool continueEditing)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var ruleSet = MiniMapper.Map<RuleSetModel, RuleSetEntity>(model);

            _ruleStorage.InsertRuleSet(ruleSet);

            NotifySuccess(T("Admin.Rules.RuleSet.Added"));

            return continueEditing ? RedirectToAction("Edit", new { id = ruleSet.Id }) : RedirectToAction("List");
        }

        [Permission(Permissions.System.Rule.Read)]
        public ActionResult Edit(int id /* ruleSetId */)
        {
            var entity = _ruleStorage.GetRuleSetById(id, false, true);
            if (entity == null || entity.IsSubGroup)
            {
                return HttpNotFound();
            }

            var model = MiniMapper.Map<RuleSetEntity, RuleSetModel>(entity);
            var provider = _ruleProvider(entity.Scope);
            
            model.ExpressionGroup = _ruleFactory.CreateExpressionGroup(entity, provider);
            model.AvailableDescriptors = _targetGroupService.RuleDescriptors;
            
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [Permission(Permissions.System.Rule.Update)]
        public ActionResult Edit(RuleSetModel model, bool continueEditing)
        {
            var ruleSet = _ruleStorage.GetRuleSetById(model.Id, true, true);

            MiniMapper.Map(model, ruleSet);

            _ruleStorage.UpdateRuleSet(ruleSet);

            return continueEditing ? RedirectToAction("Edit", new { id = ruleSet.Id }) : RedirectToAction("List");
        }

        [HttpPost, ActionName("Delete")]
        [Permission(Permissions.System.Rule.Delete)]
        public ActionResult DeleteConfirmed(int id)
        {
            var ruleSet = _ruleStorage.GetRuleSetById(id, false, false);
            if (ruleSet == null)
            {
                return HttpNotFound();
            }

            _ruleStorage.DeleteRuleSet(ruleSet);

            NotifySuccess(T("Admin.Rules.RuleSet.Deleted"));
            return RedirectToAction("List");
        }


        [HttpPost]
        [Permission(Permissions.System.Rule.Create)]
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
        [Permission(Permissions.System.Rule.Update)]
        public ActionResult UpdateRule(int ruleId, string op, string value)
        {
            var rule = _ruleStorage.GetRuleById(ruleId, true);
            if (rule == null)
            {
                // TODO: message/notify
                return Json(new { Success = false, Message = "" });
            }

            rule.Operator = op;
            rule.Value = value;

            _ruleStorage.UpdateRule(rule);

            return Json(new { Success = true });
        }

        [HttpPost]
        [Permission(Permissions.System.Rule.Delete)]
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
        [Permission(Permissions.System.Rule.Update)]
        public ActionResult ChangeOperator(int ruleSetId, string op)
        {
            var ruleSet = _ruleStorage.GetRuleSetById(ruleSetId, false, false);

            ruleSet.LogicalOperator = op.IsCaseInsensitiveEqual("and") ? LogicalRuleOperator.And : LogicalRuleOperator.Or;

            _ruleStorage.UpdateRuleSet(ruleSet);

            return Json(new { Success = true });
        }

        [HttpPost]
        [Permission(Permissions.System.Rule.Create)]
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
        [Permission(Permissions.System.Rule.Delete)]
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

        [HttpPost]
        [Permission(Permissions.System.Rule.Execute)]
        public ActionResult Execute(int ruleSetId)
        {
            var result = _targetGroupService.ProcessFilter(new[] { ruleSetId }, 0, 500);

            return Json(new
            {
                Success = true,
                Message = $"{result.TotalCount} Kunden entsprechen dem Filter."
            });

            //return Content($"{result.TotalCount} Kunden entsprechen dem Filter.");
        }
    }
}