using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Web.Mvc;
using SmartStore.Core.Infrastructure;
using SmartStore.Services.Filter;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Web.Models.Filter;
using SmartStore.Web.Framework.Mvc;
using System.Net.Mime;
using SmartStore.Web.Models.Catalog;
using System;
using SmartStore.Services.Catalog;
using SmartStore.Services.Security;
using System.Diagnostics;
using SmartStore.Web.Infrastructure.Cache;
using SmartStore.Core;
using SmartStore.Services.Localization;
using SmartStore.Core.Domain.Tax;
using SmartStore.Services.Directory;
using SmartStore.Core.Domain.Directory;
using SmartStore.Services.Tax;
using SmartStore.Services.Media;
using SmartStore.Core.Domain.Media;
using SmartStore.Web.Models.Media;
using SmartStore.Core.Caching;
using SmartStore.Services.Seo;
using SmartStore.Web.Framework.Controllers;

namespace SmartStore.Web.Controllers
{
	/// <remarks>codehint: sm-add</remarks>
	public partial class FilterController : Controller		// not BaseController cause of performance
    {
		private readonly IFilterService _filterService;

		public FilterController(IFilterService filterService)
		{
			_filterService = filterService;
		}

		public ActionResult Products(string filter, int categoryID, string path, int? pagesize, int? orderby, string viewmode)
		{
			var context = new FilterProductContext()
			{
				Filter = filter,
				ParentCategoryID = categoryID,
				CategoryIds = new List<int> { categoryID },
				Path = path,
				PageSize = pagesize ?? 12,
				ViewMode = viewmode,
				OrderBy = orderby,
				Criteria = _filterService.Deserialize(filter)
			};
		
			_filterService.ProductFilterable(context);

			return PartialView(new ProductFilterModel { Context = context });
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
				OrderBy = orderby
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
			var context = new FilterProductContext()
			{
				Filter = filter,
				ParentCategoryID = categoryID,
				CategoryIds = new List<int> { categoryID },
				Path = path,
				PageSize = pagesize ?? 12,
				ViewMode = viewmode,
				OrderBy = orderby,
				Criteria = _filterService.Deserialize(filter)
			};

			_filterService.ProductFilterableMultiSelect(context, filterMultiSelect);

			return PartialView(new ProductFilterModel { Context = context });
		}

    }	// class
}
