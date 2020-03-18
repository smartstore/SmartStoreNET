using System.Collections.Generic;
using System.Linq;
using SmartStore.Core;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Localization;
using SmartStore.Core.Search;
using SmartStore.Rules;
using SmartStore.Rules.Domain;
using SmartStore.Rules.Filters;
using SmartStore.Services.Search;

namespace SmartStore.Services.Catalog.Rules
{
    public class ProductRuleProvider : RuleProviderBase, IProductRuleProvider
    {
        private readonly IRuleFactory _ruleFactory;
        private readonly ICommonServices _services;
        private readonly ICatalogSearchService _catalogSearchService;

        public ProductRuleProvider(
            IRuleFactory ruleFactory,
            ICommonServices services,
            ICatalogSearchService catalogSearchService)
            : base(RuleScope.Product)
        {
            _ruleFactory = ruleFactory;
            _services = services;
            _catalogSearchService = catalogSearchService;
        }

        public Localizer T { get; set; } = NullLocalizer.Instance;

        public SearchFilterExpressionGroup CreateExpressionGroup(int ruleSetId)
        {
            return _ruleFactory.CreateExpressionGroup(ruleSetId, this) as SearchFilterExpressionGroup;
        }

        public override IRuleExpression VisitRule(RuleEntity rule)
        {
            var expression = new SearchFilterExpression();
            base.ConvertRule(rule, expression);
            expression.Descriptor = ((RuleExpression)expression).Descriptor as SearchFilterDescriptor;
            return expression;
        }

        public override IRuleExpressionGroup VisitRuleSet(RuleSetEntity ruleSet)
        {
            var group = new SearchFilterExpressionGroup()
            {
                Id = ruleSet.Id,
                LogicalOperator = ruleSet.LogicalOperator,
                IsSubGroup = ruleSet.IsSubGroup,
                Value = ruleSet.Id,
                RawValue = ruleSet.Id.ToString(),
                Provider = this
            };

            return group;
        }

        public IPagedList<Product> Search(SearchFilterExpression[] filters, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            if ((filters?.Length ?? 0) == 0)
            {
                return new PagedList<Product>(Enumerable.Empty<Product>(), 0, int.MaxValue);
            }

            SearchFilterExpressionGroup group;

            if (filters.Length == 1 && filters[0] is SearchFilterExpressionGroup group2)
            {
                group = group2;
            }
            else
            {
                group = new SearchFilterExpressionGroup();
                group.AddExpressions(filters);
            }

            var searchQuery = new CatalogSearchQuery()
                .OriginatesFrom("Rule/Search")
                .WithLanguage(_services.WorkContext.WorkingLanguage)
                .WithCurrency(_services.WorkContext.WorkingCurrency)
                .BuildFacetMap(false)
                .CheckSpelling(0)
                .Slice(pageIndex * pageSize, pageSize)
                .SortBy(ProductSortingEnum.CreatedOn);

            searchQuery = group.ApplyFilters(searchQuery);

            var searchResult = _catalogSearchService.Search(searchQuery);

            return searchResult.Hits;
        }

        protected override IEnumerable<RuleDescriptor> LoadDescriptors()
        {
            var stores = _services.StoreService.GetAllStores()
                .Select(x => new RuleValueSelectListOption { Value = x.Id.ToString(), Text = x.Name })
                .ToArray();

            var descriptors = new List<SearchFilterDescriptor>
            {
                new SearchFilterDescriptor<bool>(x => SearchFilter.ByField("published", x).Mandatory().ExactMatch().NotAnalyzed())
                {
                    Name = "Published",
                    DisplayName = T("Admin.Rules.FilterDescriptor.Published"),
                    RuleType = RuleType.Boolean,
                    Operators = new RuleOperator[] { RuleOperator.IsEqualTo }
                },
                new SearchFilterDescriptor<int>(x => x > 0 ? SearchFilter.Combined(SearchFilter.ByField("storeid", 0).ExactMatch().NotAnalyzed(), SearchFilter.ByField("storeid", x).ExactMatch().NotAnalyzed()) : null)
                {
                    Name = "Store",
                    DisplayName = T("Admin.Rules.FilterDescriptor.Store"),
                    RuleType = RuleType.Int,
                    SelectList = new LocalRuleValueSelectList(stores),
                    Operators = new RuleOperator[] { RuleOperator.IsEqualTo }
                },
                new SearchFilterDescriptor<int[]>(x => SearchFilter.Combined(x.Select(id => SearchFilter.ByField("manufacturerid", id).ExactMatch().NotAnalyzed()).ToArray()))
                {
                    Name = "Manufacturer",
                    DisplayName = T("Admin.Rules.FilterDescriptor.Manufacturer"),
                    RuleType = RuleType.IntArray,
                    SelectList = new RemoteRuleValueSelectList("Manufacturer") { Multiple = true },
                    Operators = new RuleOperator[] { RuleOperator.In }
                },
                // TODO: HasParent(parentId)
                new SearchFilterDescriptor<int[]>(x => SearchFilter.Combined(x.Select(id => SearchFilter.ByField("attrvalueid", id).ExactMatch().NotAnalyzed()).ToArray()))
                {
                    Name = "Attribute",
                    DisplayName = T("Admin.Rules.FilterDescriptor.SpecificationAttribute"),
                    RuleType = RuleType.IntArray,
                    LeftSelectList = new RemoteRuleValueSelectList("Attribute"),
                    SelectList = new RemoteRuleValueSelectList("AttributeOption") { Multiple = true },
                    Operators = new RuleOperator[] { RuleOperator.In }
                },
            };

            descriptors
                .Where(x => x.RuleType == RuleType.Money)
                .Each(x => x.Metadata["postfix"] = _services.StoreContext.CurrentStore.PrimaryStoreCurrency.CurrencyCode);

            return descriptors;
        }
    }
}
