using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Services.Filter;
using SmartStore.Web.Models.Filter;

namespace SmartStore.Web.Controllers
{
	public partial class FilterController : Controller // not BaseController cause of performance
    {
		private readonly IFilterService _filterService;
        private readonly CatalogSettings _catalogSettings;

		public FilterController(IFilterService filterService, CatalogSettings catalogSettings)
		{
			_filterService = filterService;
            _catalogSettings = catalogSettings;
		}

		public ActionResult Products(string filter, int categoryID, string path, int? pagesize, int? orderby, string viewmode)
		{
			var context = _filterService.CreateFilterProductContext(filter, categoryID, path, pagesize, orderby, viewmode);
		
			_filterService.ProductFilterable(context);

            return PartialView(new ProductFilterModel 
			{ 
                Context = context, 
                IsShowAllText = IsShowAllText(context.Criteria),
                MaxFilterItemsToDisplay = _catalogSettings.MaxFilterItemsToDisplay,
                ExpandAllFilterGroups = _catalogSettings.ExpandAllFilterCriteria
            });
		}

		[HttpPost]
		public ActionResult Products(string active, string inactive, int categoryID, int? pagesize, int? orderby, string viewmode)
		{
			// TODO: needed later for ajax based filtering... see example below
			//System.Threading.Thread.Sleep(3000);

			var context = new FilterProductContext()
			{
				ParentCategoryID = categoryID,
				CategoryIds = new List<int> { categoryID },
				Criteria = _filterService.Deserialize(active),
				OrderBy = orderby,
			};

			context.Criteria.AddRange(_filterService.Deserialize(inactive));

			//var query = _filterService.ProductFilter(context);
			//var products = new PagedList<Product>(query, 0, pagesize ?? 4);

			//ProductListModel model = new ProductListModel {
			//	PagingFilteringContext = new CatalogPagingFilteringModel()
			//};

			//model.Products = PrepareProductOverviewModels(products).ToList();
			//model.PagingFilteringContext.LoadPagedList(products);

			//string htmlProducts = this.RenderPartialViewToString("~/Views/Catalog/_ProductBoxContainer.cshtml", model);

			//return Json(new {
			//	products = htmlProducts
			//});

			return null;
		}

		public ActionResult ProductsMultiSelect(string filter, int categoryID, string path, int? pagesize, int? orderby, string viewmode, string filterMultiSelect)
		{
			var context = _filterService.CreateFilterProductContext(filter, categoryID, path, pagesize, orderby, viewmode);

			_filterService.ProductFilterableMultiSelect(context, filterMultiSelect);

            return PartialView(new ProductFilterModel { 
                Context = context, 
                IsShowAllText = IsShowAllText(context.Criteria),
                MaxFilterItemsToDisplay = _catalogSettings.MaxFilterItemsToDisplay,
                ExpandAllFilterGroups = _catalogSettings.ExpandAllFilterCriteria
            });
		}

		private bool IsShowAllText(ICollection<FilterCriteria> criteriaGroup)
		{
			if (criteriaGroup.Any(c => c.Entity == FilterService.ShortcutPrice))
				return false;

			return (criteriaGroup.Count >= _catalogSettings.MaxFilterItemsToDisplay || criteriaGroup.Any(c => !c.IsInactive));
		}
    }
}
