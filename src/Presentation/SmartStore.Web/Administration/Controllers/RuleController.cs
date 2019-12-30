using System;
using System.Collections.Generic;
using System.Globalization;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Admin.Models.Rules;
using SmartStore.ComponentModel;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Search;
using SmartStore.Core.Security;
using SmartStore.Rules;
using SmartStore.Rules.Domain;
using SmartStore.Services.Customers;
using SmartStore.Services.Directory;
using SmartStore.Services.Localization;
using SmartStore.Services.Search;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Filters;
using SmartStore.Web.Framework.Security;
using Telerik.Web.Mvc;
using SmartStore.Services.Catalog;

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
        private readonly Lazy<ICatalogSearchService> _catalogSearchService;
        private readonly Lazy<IProductService> _productService;
        private readonly Lazy<SearchSettings> _searchSettings;

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
            Lazy<ICountryService> countryService,
            Lazy<ICatalogSearchService> catalogSearchService,
            Lazy<IProductService> productService,
            Lazy<SearchSettings> searchSettings)
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
            _catalogSearchService = catalogSearchService;
            _productService = productService;
            _searchSettings = searchSettings;
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

            PrepareTemplateViewBag(0);

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

            PrepareExpressions(model.ExpressionGroup);
            PrepareTemplateViewBag(entity.Id);

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

            PrepareTemplateViewBag(ruleSetId);

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
            int ruleSetId,
            string ruleType,
            string selected,
            string search,
            int? page)
        {
            var language = Services.WorkContext.WorkingLanguage;
            var selectedArr = selected.SplitSafe(",");
            List<RuleSelectItem> data = null;

            // Load all data by default.
            const int pageSize = 200;
            var hasPaging = false;
            var hasMoreItems = false;

            switch (dataSource)
            {
                case "Country":
                    var byId = ruleType == RuleType.IntArray.Name;
                    data = _countryService.Value.GetAllCountries(true)
                        .Select(x => new RuleSelectItem { id = byId ? x.Id.ToString() : x.TwoLetterIsoCode, text = x.GetLocalized(y => y.Name, language, true, false) })
                        .ToList();
                    break;
                case "Currency":
                    data = _currencyService.Value.GetAllCurrencies(true)
                        .Select(x => new RuleSelectItem { id = x.Id.ToString(), text = x.GetLocalized(y => y.Name, language, true, false) })
                        .ToList();
                    break;
                case "CustomerRole":
                    data = _customerService.Value.GetAllCustomerRoles(true)
                        .Select(x => new RuleSelectItem { id = x.Id.ToString(), text = x.Name })
                        .ToList();
                    break;
                case "Language":
                    data = _languageService.Value.GetAllLanguages(true)
                        .Select(x => new RuleSelectItem { id = x.Id.ToString(), text = GetCultureDisplayName(x) ?? x.Name })
                        .ToList();
                    break;
                case "Store":
                    data = Services.StoreService.GetAllStores()
                        .Select(x => new RuleSelectItem { id = x.Id.ToString(), text = x.Name.NaIfEmpty() })
                        .ToList();
                    break;
                case "CartRule":
                case "TargetGroup":
                    // This can only work if the other rule set is of the same scope as the current.
                    var scope = dataSource == "TargetGroup" ? RuleScope.Customer : RuleScope.Cart;
                    var pagedData = _ruleStorage.GetAllRuleSets(false, false, scope, page ?? 0, pageSize, includeHidden: true);
                    hasPaging = true;
                    hasMoreItems = pagedData.HasNextPage;
                    data = pagedData
                        .Where(x => x.Id != ruleSetId)
                        .Select(x => new RuleSelectItem { id = x.Id.ToString(), text = x.Name.NaIfEmpty() })
                        .ToList();
                    break;
                case "Product":
                    hasPaging = true;
                    data = SearchProducts(search, (page ?? 0) * pageSize, pageSize, out hasMoreItems);                    
                    break;
                default:
                    data = new List<RuleSelectItem>();
                    break;
            }

            // Mark selected items.
            data.Each(x => x.selected = selectedArr.Contains(x.id));

            // Apply search term to non-paged data.
            if (!hasPaging && search.HasValue() && data.Any())
            {
                data = data.Where(x => x.text?.Contains(search) ?? true).ToList();
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

        #region Rule template and value helpers

        private void PrepareExpressions(IRuleExpressionGroup expressionGroup)
        {
            var language = Services.WorkContext.WorkingLanguage;
            // Load all display names for certain entities.
            var allDisplayNames = new Dictionary<string, Dictionary<string, string>>();

            PrepareGroup(expressionGroup);

            void PrepareGroup(IRuleExpressionGroup group)
            {
                if (group == null)
                {
                    return;
                }

                foreach (var expression in group.Expressions)
                {
                    if (expression is IRuleExpressionGroup subGroup)
                    {
                        PrepareGroup(subGroup);
                        continue;
                    }

                    var d = expression.Descriptor;

                    // Add options for selected values.
                    if (expression.RawValue.HasValue() && d.SelectList is RemoteRuleValueSelectList list)
                    {
                        var selected = new Dictionary<string, string>();
                        Dictionary<string, string> names = null;

                        switch (list.DataSource)
                        {
                            case "Country":
                                var byId = d.RuleType == RuleType.Int || d.RuleType == RuleType.IntArray;
                                names = allDisplayNames[byId ? "CountryById" : "CountryByIsoCode"] = _countryService.Value.GetAllCountries(true)
                                    .ToDictionary(x => byId ? x.Id.ToString() : x.TwoLetterIsoCode, x => x.GetLocalized(y => y.Name, language, true, false).Value);
                                break;
                            case "Currency":
                                names = allDisplayNames[list.DataSource] = _currencyService.Value.GetAllCurrencies(true)
                                    .ToDictionary(x => x.Id.ToString(), x => x.GetLocalized(y => y.Name, language, true, false).Value);
                                break;
                            case "CustomerRole":
                                names = allDisplayNames[list.DataSource] = _customerService.Value.GetAllCustomerRoles(true)
                                    .ToDictionary(x => x.Id.ToString(), x => x.Name);
                                break;
                            case "Language":
                                names = allDisplayNames[list.DataSource] = _languageService.Value.GetAllLanguages(true)
                                    .ToDictionary(x => x.Id.ToString(), x => GetCultureDisplayName(x) ?? x.Name);
                                break;
                            case "Store":
                                names = allDisplayNames[list.DataSource] = Services.StoreService.GetAllStores()
                                    .ToDictionary(x => x.Id.ToString(), x => x.Name);
                                break;
                            case "CartRule":
                            case "TargetGroup":
                                names = _ruleStorage.GetRuleSetsByIds(expression.RawValue.ToIntArray(), false)
                                    .ToDictionary(x => x.Id.ToString(), x => x.Name);
                                break;
                            case "Product":
                                names = _productService.Value.GetProductsByIds(expression.RawValue.ToIntArray())
                                    .ToDictionary(x => x.Id.ToString(), x => x.GetLocalized(y => y.Name, language, true, false).Value);
                                break;
                        }

                        if (names?.Any() ?? false)
                        {
                            foreach (var value in expression.RawValue.SplitSafe(","))
                            {
                                if (names.TryGetValue(value, out var name))
                                {
                                    selected[value] = name;
                                }
                            }
                        }

                        expression.Metadata["SelectedDisplayNames"] = selected;
                    }
                }
            }
        }

        private void PrepareTemplateViewBag(int ruleSetId)
        {
            ViewBag.ruleSetId = ruleSetId;
            ViewBag.TemplateSelector = _ruleTemplateSelector;
            //ViewBag.LanguageSeoCode = Services.WorkContext.WorkingLanguage.UniqueSeoCode.EmptyNull().ToLower();
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

        private List<RuleSelectItem> SearchProducts(string term, int skip, int take, out bool hasMoreItems)
        {
            List<RuleSelectItem> products;
            var fields = new List<string> { "name" };

            if (_searchSettings.Value.SearchFields.Contains("sku"))
            {
                fields.Add("sku");
            }
            if (_searchSettings.Value.SearchFields.Contains("shortdescription"))
            {
                fields.Add("shortdescription");
            }

            var searchQuery = new CatalogSearchQuery(fields.ToArray(), term);

            if (_searchSettings.Value.UseCatalogSearchInBackend)
            {
                searchQuery = searchQuery
                    .Slice(skip, take)
                    .SortBy(ProductSortingEnum.NameAsc);

                var searchResult = _catalogSearchService.Value.Search(searchQuery);
                hasMoreItems = searchResult.Hits.HasNextPage;

                // TODO: SKU
                products = searchResult.Hits
                    .Select(x => new RuleSelectItem
                    {
                        id = x.Id.ToString(),
                        text = x.Name
                    })
                    .ToList();
            }
            else
            {
                var query = _catalogSearchService.Value.PrepareQuery(searchQuery);
                
                var pageIndex = take == 0 ? 0 : Math.Max(skip / take, 0);
                hasMoreItems = (pageIndex + 1) * take < query.Count();

                products = query
                    .Select(x => new RuleSelectItem
                    {
                        id = x.Id.ToString(),
                        text = x.Name
                    })
                    .OrderBy(x => x.text)
                    .Skip(() => skip)
                    .Take(() => take)
                    .ToList();
            }

            return products;
        }

        #endregion
    }

    internal class RuleSelectItem
    {
        public string id { get; set; }
        public string text { get; set; }
        public bool selected { get; set; }
    }
}