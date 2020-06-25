using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
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
            var pageSize = 500;
            var categoryQuery = _categoryRepository.TableUntracked.Expand(x => x.RuleSets);

            if (ctx.Parameters.ContainsKey("CategoryIds"))
            {
                var categoryIds = ctx.Parameters["CategoryIds"].ToIntArray();
                categoryQuery = categoryQuery.Where(x => categoryIds.Contains(x.Id));

                numDeleted = _productCategoryRepository.Context.ExecuteSqlCommand(
                    "Delete From [dbo].[Product_Category_Mapping] Where [CategoryId] In ({0}) And [IsSystemMapping] = 1",
                    false,
                    null,
                    string.Join(",", categoryIds));
            }
            else
            {
                numDeleted = _productCategoryRepository.Context.ExecuteSqlCommand("Delete From [dbo].[Product_Category_Mapping] Where [IsSystemMapping] = 1");
            }

            var categories = await categoryQuery
                .Where(x => x.Published && !x.Deleted && x.RuleSets.Any(y => y.IsActive))
                .ToListAsync();

            using (var scope = new DbContextScope(ctx: _productCategoryRepository.Context, autoDetectChanges: false, validateOnSave: false, hooksEnabled: false, autoCommit: false))
            {
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
                            var pageIndex = -1;
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

            Debug.WriteLineIf(numDeleted > 0 || numAdded > 0, $"Deleted {numDeleted} and added {numAdded} product mappings for {categories.Count} categories.");
        }
    }
}
