using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Data.Utilities;
using SmartStore.Rules;
using SmartStore.Services.Catalog.Rules;
using SmartStore.Services.Tasks;

namespace SmartStore.Services.Catalog
{
    /// <summary>
    /// Updates the system assignments to categories for rules.
    /// </summary>
    public partial class ProductRuleEvaluatorTask : AsyncTask
    {
        protected readonly IRepository<Category> _categoryRepository;
        protected readonly IRepository<ProductCategory> _productCategoryRepository;
        protected readonly IRuleFactory _ruleFactory;
        protected readonly IProductRuleProvider _productRuleProvider;

        public ProductRuleEvaluatorTask(
            IRepository<Category> categoryRepository,
            IRepository<ProductCategory> productCategoryRepository,
            IRuleFactory ruleFactory,
            IProductRuleProvider productRuleProvider)
        {
            _categoryRepository = categoryRepository;
            _productCategoryRepository = productCategoryRepository;
            _ruleFactory = ruleFactory;
            _productRuleProvider = productRuleProvider;
        }

        public override async Task ExecuteAsync(TaskExecutionContext ctx)
        {
            var count = 0;
            var numDeleted = 0;
            var numAdded = 0;
            var numCategories = 0;
            var pageSize = 500;
            var pageIndex = -1;

            var categoryIds = ctx.Parameters.ContainsKey("CategoryIds")
                ? ctx.Parameters["CategoryIds"].ToIntArray()
                : null;

            // Hooks are enabled because search index needs to be updated.
            using (var scope = new DbContextScope(ctx: _productCategoryRepository.Context, autoDetectChanges: false, validateOnSave: false, hooksEnabled: true, autoCommit: false))
            {
                // Delete existing system mappings.
                var deleteQuery = _productCategoryRepository.Table.Where(x => x.IsSystemMapping);
                if (categoryIds != null)
                {
                    deleteQuery = deleteQuery.Where(x => categoryIds.Contains(x.CategoryId));
                }

                var pager = new FastPager<ProductCategory>(deleteQuery, pageSize);

                while (pager.ReadNextPage(out var mappings))
                {
                    if (ctx.CancellationToken.IsCancellationRequested)
                        return;

                    if (mappings.Any())
                    {
                        _productCategoryRepository.DeleteRange(mappings);
                        numDeleted += await scope.CommitAsync();
                    }
                }

                try
                {
                    _productCategoryRepository.Context.DetachEntities<ProductCategory>();
                }
                catch { }

                // Insert new product category mappings.
                var categoryQuery = _categoryRepository.TableUntracked.Expand(x => x.RuleSets);
                if (categoryIds != null)
                {
                    categoryQuery = categoryQuery.Where(x => categoryIds.Contains(x.Id));
                }

                var categories = await categoryQuery
                    .Where(x => x.Published && !x.Deleted && x.RuleSets.Any(y => y.IsActive))
                    .ToListAsync();

                numCategories = categories.Count;

                foreach (var category in categories)
                {
                    var ruleSetProductIds = new HashSet<int>();

                    ctx.SetProgress(++count, categories.Count, $"Add product mappings for category \"{category.Name.NaIfEmpty()}\".");

                    // Execute active rule sets and collect product ids.
                    foreach (var ruleSet in category.RuleSets.Where(x => x.IsActive))
                    {
                        if (ctx.CancellationToken.IsCancellationRequested)
                            return;

                        if (_ruleFactory.CreateExpressionGroup(ruleSet, _productRuleProvider) is SearchFilterExpression expression)
                        {
                            pageIndex = -1;
                            while (true)
                            {
                                // Do not touch searchResult.Hits. We only need the product identifiers.
                                var searchResult = _productRuleProvider.Search(new SearchFilterExpression[] { expression }, ++pageIndex, pageSize);
                                ruleSetProductIds.AddRange(searchResult.HitsEntityIds);

                                if (pageIndex >= (searchResult.TotalHitsCount / pageSize))
                                {
                                    break;
                                }
                            }
                        }
                    }

                    // Add mappings.
                    if (ruleSetProductIds.Any())
                    {
                        foreach (var chunk in ruleSetProductIds.Slice(500))
                        {
                            if (ctx.CancellationToken.IsCancellationRequested)
                                return;

                            foreach (var productId in chunk)
                            {
                                _productCategoryRepository.Insert(new ProductCategory
                                {
                                    ProductId = productId,
                                    CategoryId = category.Id,
                                    IsSystemMapping = true
                                });

                                ++numAdded;
                            }

                            await scope.CommitAsync();
                        }

                        try
                        {
                            _productCategoryRepository.Context.DetachEntities<ProductCategory>();
                        }
                        catch { }
                    }
                }
            }

            Debug.WriteLineIf(numDeleted > 0 || numAdded > 0, $"Deleted {numDeleted} and added {numAdded} product mappings for {numCategories} categories.");
        }
    }
}
