using SmartStore.Admin.Models.Rules;
using SmartStore.ComponentModel;
using SmartStore.Rules;
using SmartStore.Rules.Domain;
using SmartStore.Services.Cart.Rules;
using SmartStore.Services.Customers;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Filters;
using SmartStore.Web.Framework.Security;
using System;
using System.Linq;
using System.Web.Mvc;
using Telerik.Web.Mvc;

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
            // TODO: check permission
            var model = new RuleSetListModel();

            foreach (var s in Services.StoreService.GetAllStores())
            {
                model.AvailableStores.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString() });
            }

            return View(model);
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult List(GridCommand command, RuleSetListModel model)
        {
            // TODO: check permission

            var gridModel = new GridModel<RuleSetModel>();

            var ruleSets = _ruleStorage.GetAllRuleSets(false, false);

            gridModel.Data = ruleSets.Select(x =>
            {
                var item = new RuleSetModel();

                MiniMapper.Map(x, item);

                return item;
            });

            gridModel.Total = ruleSets.TotalCount;

            return new JsonResult
            {
                MaxJsonLength = int.MaxValue,
                Data = gridModel
            };
        }

        public ActionResult Create()
        {
            // TODO: check permission

            var model = new RuleSetModel();

            model.ExpressionGroup = _ruleFactory.CreateExpressionGroup(new RuleSetEntity { Scope = RuleScope.Customer }, _targetGroupService);
            model.AvailableDescriptors = _targetGroupService.RuleDescriptors;

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [ValidateInput(false)]
        public ActionResult Create(RuleSetModel model, bool continueEditing, FormCollection form)
        {
            // TODO: check permission

            if (!ModelState.IsValid)
                return View(model);

            var ruleSet = new RuleSetEntity();

            MiniMapper.Map(model, ruleSet);

            _ruleStorage.InsertRuleSet(ruleSet);

            NotifySuccess(T("Admin.ContentManagement.RuleSet.Added"));

            return continueEditing ? RedirectToAction("Edit", new { id = ruleSet.Id }) : RedirectToAction("List");                        
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

            // TODO: delete later. For now there seems to be an error if the scope isn't Customer
            if (model.ExpressionGroup == null)
            {
                model.ExpressionGroup = _ruleFactory.CreateExpressionGroup(new RuleSetEntity { Scope = RuleScope.Customer }, _targetGroupService);
            }
            
            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(RuleSetModel model)
        {
            var ruleSet = _ruleStorage.GetRuleSetById(model.Id, true, true);

            MiniMapper.Map(model, ruleSet);

            _ruleStorage.UpdateRuleSet(ruleSet);

            return RedirectToAction("Edit", new { id = model.Id });
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
        public ActionResult ChangeOperator(int ruleSetId, string op)
        {
            var ruleSet = _ruleStorage.GetRuleSetById(ruleSetId, false, false);

            ruleSet.LogicalOperator = op.IsCaseInsensitiveEqual("and") ? LogicalRuleOperator.And : LogicalRuleOperator.Or;

            _ruleStorage.UpdateRuleSet(ruleSet);

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

        [HttpPost]
        public ActionResult Execute(int ruleSetId)
        {
            var result = _targetGroupService.ProcessFilter(new[] { ruleSetId }, 0, 500);

            return Json(new {
                Success = true,
                Message = $"{result.TotalCount} Kunden entsprechen dem Filter."
            });

            //return Content($"{result.TotalCount} Kunden entsprechen dem Filter.");
        }
    }
}