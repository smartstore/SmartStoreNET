using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Admin.Models.Rules;
using SmartStore.ComponentModel;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Security;
using SmartStore.Rules;
using SmartStore.Rules.Domain;
using SmartStore.Services.Customers;
using SmartStore.Services.Directory;
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
        private readonly IRuleTemplateSelector _ruleTemplateSelector;
        private readonly Func<RuleScope, IRuleProvider> _ruleProvider;
        private readonly AdminAreaSettings _adminAreaSettings;

        private readonly Lazy<ICurrencyService> _currencyService;
        private readonly Lazy<ICustomerService> _customerService;
        private readonly Lazy<ILanguageService> _languageService;
        private readonly Lazy<ICountryService> _countryService;

        public RuleController(
            IRuleFactory ruleFactory,
            IRuleStorage ruleStorage,
            ITargetGroupService targetGroupService,
            IRuleTemplateSelector ruleTemplateSelector,
            Func<RuleScope, IRuleProvider> ruleProvider,
            AdminAreaSettings adminAreaSettings,
            Lazy<ICurrencyService> currencyService,
            Lazy<ICustomerService> customerService,
            Lazy<ILanguageService> languageService,
            Lazy<ICountryService> countryService)
        {
            _ruleFactory = ruleFactory;
            _ruleStorage = ruleStorage;
            _targetGroupService = targetGroupService;
            _ruleTemplateSelector = ruleTemplateSelector;
            _ruleProvider = ruleProvider;
            _adminAreaSettings = adminAreaSettings;

            _currencyService = currencyService;
            _customerService = customerService;
            _languageService = languageService;
            _countryService = countryService;
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

            PrepareTemplateViewBag();

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

            //PrepareExpressionGroup(model.ExpressionGroup);
            PrepareTemplateViewBag();

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

            PrepareTemplateViewBag();

            return PartialView("_Rule", expression);
        }

        [HttpPost]
        [Permission(Permissions.System.Rule.Update)]
        public ActionResult UpdateRule(int ruleId, string op, string value)
        {
            var rule = _ruleStorage.GetRuleById(ruleId, true);
            if (rule == null)
            {
                NotifyError(T("Admin.Rules.NotFound", ruleId));
                return Json(new { Success = false });
            }

            // TODO? Ugly. There should be a better way. Do not store culture variant values.
            if (value.HasValue())
            {
                var provider = _ruleProvider(rule.RuleSet.Scope);
                var descriptor = provider.RuleDescriptors.FindDescriptor(rule.RuleType);

                if (descriptor.RuleType == RuleType.Money)
                {
                    value = value.Convert<decimal>(CultureInfo.CurrentCulture).ToString(CultureInfo.InvariantCulture);
                }
                else if (descriptor.RuleType == RuleType.Float || descriptor.RuleType == RuleType.NullableFloat)
                {
                    value = value.Convert<float>(CultureInfo.CurrentCulture).ToString(CultureInfo.InvariantCulture);
                }
                else if (descriptor.RuleType == RuleType.DateTime || descriptor.RuleType == RuleType.NullableDateTime)
                {
                    value = value.Convert<DateTime>(CultureInfo.CurrentCulture).ToString(CultureInfo.InvariantCulture);
                }
            }
            //if (value?.Contains(',') ?? false)
            //{
            //    var provider = _ruleProvider(rule.RuleSet.Scope);
            //    var descriptor = provider.RuleDescriptors.FindDescriptor(rule.RuleType);
            //    var floatingPointTypes = new Type[] { typeof(decimal), typeof(decimal?), typeof(float), typeof(float?), typeof(double), typeof(double?) };

            //    if (floatingPointTypes.Contains(descriptor.RuleType.ClrType))
            //    {
            //        value = value.Replace(",", ".");
            //    }
            //}

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
                NotifyError(T("Admin.Rules.NotFound", ruleId));
                return Json(new { Success = false });
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
            expression.RefRuleId = groupRefRule.Id;

            return PartialView("_RuleSet", expression);
        }

        [HttpPost]
        [Permission(Permissions.System.Rule.Delete)]
        public ActionResult DeleteGroup(int refRuleId)
        {
            var refRule = _ruleStorage.GetRuleById(refRuleId, true);
            var ruleSetId = refRule?.Value?.ToInt() ?? 0;

            var group = _ruleStorage.GetRuleSetById(ruleSetId, true, false);
            if (group == null)
            {
                NotifyError(T("Admin.Rules.GroupNotFound", ruleSetId));
                return Json(new { Success = false });
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

        // Ajax.
        public ActionResult RuleValues(
            string dataSource,
            string ruleType,
            string selected,
            int? page)
        {
            var language = Services.WorkContext.WorkingLanguage;
            var selectedArr = selected.SplitSafe(",");
            dynamic data = new List<object>();

            // Load all data by default.
            page = page ?? 1;
            const int take = 200;
            var skip = (page.Value - 1) * take;
            var hasMoreItems = false;
            //var hasMoreItems = (page.Value * take) < categories.Count;

            switch (dataSource)
            {
                case "Country":
                    if (ruleType == RuleType.IntArray.Name)
                    {
                        data = _countryService.Value.GetAllCountries(true)
                            .Select(x => new
                            {
                                id = x.Id,
                                text = x.GetLocalized(y => y.Name, language, true, false).Value,
                                selected = selectedArr.Contains(x.Id.ToString())
                            })
                            .ToList();
                    }
                    else
                    {
                        data = _countryService.Value.GetAllCountries(true)
                            .Select(x => new
                            {
                                id = x.TwoLetterIsoCode,
                                text = x.GetLocalized(y => y.Name, language, true, false).Value,
                                selected = selectedArr.Contains(x.TwoLetterIsoCode)
                            })
                            .ToList();
                    }
                    break;
                case "Currency":
                    data = _currencyService.Value.GetAllCurrencies(true)
                        .Select(x => new
                        {
                            id = x.Id,
                            text = x.GetLocalized(y => y.Name, language, true, false).Value,
                            selected = selectedArr.Contains(x.Id.ToString())
                        })
                        .ToList();
                    break;
                case "CustomerRole":
                    data = _customerService.Value.GetAllCustomerRoles(true)
                        .Select(x => new
                        {
                            id = x.Id,
                            text = x.Name,
                            selected = selectedArr.Contains(x.Id.ToString())
                        })
                        .ToList();
                    break;
                case "Language":
                    data = _languageService.Value.GetAllLanguages(true)
                        .Select(x => new
                        {
                            id = x.Id,
                            text = GetCultureDisplayName(x) ?? x.Name,
                            selected = selectedArr.Contains(x.Id.ToString())
                        })
                        .ToList();
                    break;
                case "Store":
                    data = Services.StoreService.GetAllStores()
                        .Select(x => new
                        {
                            id = x.Id,
                            text = x.Name.NaIfEmpty(),
                            selected = selectedArr.Contains(x.Id.ToString())
                        })
                        .ToList();
                    break;
            }

            return new JsonResult
            {
                Data = new
                {
                    hasMoreItems,
                    results = data
                },
                MaxJsonLength = int.MaxValue,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        //private void PrepareExpressionGroup(IRuleExpressionGroup group)
        //{
        //    if (group == null)
        //    {
        //        return;
        //    }

        //    foreach (var expression in group.Expressions)
        //    {
        //        if (expression is IRuleExpressionGroup subGroup)
        //        {
        //            PrepareExpressionGroup(subGroup);
        //            continue;
        //        }

        //        var d = expression.Descriptor;

        //        // Add options for selected values.
        //        if (d.SelectList is RemoteRuleValueSelectList remoteList)
        //        {
        //        }
        //    }
        //}

        private void PrepareTemplateViewBag()
        {
            ViewBag.TemplateSelector = _ruleTemplateSelector;
            ViewBag.LanguageSeoCode = Services.WorkContext.WorkingLanguage.UniqueSeoCode.EmptyNull().ToLower();
        }

        private string GetCultureDisplayName(Language language)
        {
            if (language?.LanguageCulture?.HasValue() ?? false)
            {
                try
                {
                    return new CultureInfo(language.LanguageCulture).DisplayName;
                }
                catch { }
            }

            return null;
        }
    }
}