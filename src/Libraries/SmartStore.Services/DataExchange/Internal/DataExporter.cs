using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Domain.Messages;
using SmartStore.Core.Localization;
using SmartStore.Services.Catalog;
using SmartStore.Services.Directory;
using SmartStore.Services.Helpers;
using SmartStore.Services.Localization;
using SmartStore.Services.Media;
using SmartStore.Services.Seo;
using SmartStore.Services.Tasks;
using SmartStore.Services.Tax;

namespace SmartStore.Services.DataExchange.Internal
{
	public partial class DataExporter : IDataExporter
	{
		private static readonly ReaderWriterLockSlim _rwLock = new ReaderWriterLockSlim();

		private readonly ICommonServices _services;
		private readonly IUrlRecordService _urlRecordService;
		private readonly ILocalizedEntityService _localizedEntityService;
		private readonly IPictureService _pictureService;
		private readonly IPriceCalculationService _priceCalculationService;
		private readonly ICurrencyService _currencyService;
		private readonly ITaxService _taxService;
		private readonly IPriceFormatter _priceFormatter;
		private readonly ICategoryService _categoryService;
		private readonly IProductAttributeParser _productAttributeParser;
		private readonly IProductService _productService;
		private readonly IDateTimeHelper _dateTimeHelper;

		private MediaSettings _mediaSettings;

		public DataExporter(
			ICommonServices services,
			IUrlRecordService urlRecordService,
			ILocalizedEntityService localizedEntityService,
			IPictureService pictureService,
			IPriceCalculationService priceCalculationService,
			ICurrencyService currencyService,
			ITaxService taxService,
			IPriceFormatter priceFormatter,
			ICategoryService categoryService,
			IProductAttributeParser productAttributeParser,
			IProductService productService,
			IDateTimeHelper dateTimeHelper,
			MediaSettings mediaSettings)
		{
			_services = services;
			_urlRecordService = urlRecordService;
			_localizedEntityService = localizedEntityService;
			_pictureService = pictureService;
			_priceCalculationService = priceCalculationService;
			_currencyService = currencyService;
			_taxService = taxService;
			_priceFormatter = priceFormatter;
			_categoryService = categoryService;
			_productAttributeParser = productAttributeParser;
			_productService = productService;
			_dateTimeHelper = dateTimeHelper;

			_mediaSettings = mediaSettings;
		}

		#region Utilities

		private void SetProgress(DataExportTaskContext ctx, int loadedRecords)
		{
			if (!ctx.IsPreview && ctx.TaskContext.ScheduleTask != null && loadedRecords > 0)
			{
				int totalRecords = ctx.RecordsPerStore.Sum(x => x.Value);

				if (ctx.Profile.Limit > 0 && totalRecords > ctx.Profile.Limit)
					totalRecords = ctx.Profile.Limit;

				ctx.RecordCount = Math.Min(ctx.RecordCount + loadedRecords, totalRecords);

				var msg = ctx.ProgressInfo.FormatInvariant(ctx.RecordCount, totalRecords);

				ctx.TaskContext.SetProgress(ctx.RecordCount, totalRecords, msg, true);
			}
		}

		private void SetProgress(DataExportTaskContext ctx, string message)
		{
			if (!ctx.IsPreview && ctx.TaskContext.ScheduleTask != null && message.HasValue())
			{
				ctx.TaskContext.SetProgress(null, message, true);
			}
		}

		#endregion

		#region Getting data

		private IQueryable<Product> GetProductQuery(DataExportTaskContext ctx, int skip, int take)
		{
			IQueryable<Product> query = null;

			if (ctx.QueryProducts == null)
			{
				var searchContext = new ProductSearchContext
				{
					OrderBy = ProductSortingEnum.CreatedOn,
					ProductIds = ctx.EntityIdsSelected,
					StoreId = (ctx.Profile.PerStore ? ctx.Store.Id : ctx.Filter.StoreId),
					VisibleIndividuallyOnly = true,
					PriceMin = ctx.Filter.PriceMinimum,
					PriceMax = ctx.Filter.PriceMaximum,
					IsPublished = ctx.Filter.IsPublished,
					WithoutCategories = ctx.Filter.WithoutCategories,
					WithoutManufacturers = ctx.Filter.WithoutManufacturers,
					ManufacturerId = ctx.Filter.ManufacturerId ?? 0,
					FeaturedProducts = ctx.Filter.FeaturedProducts,
					ProductType = ctx.Filter.ProductType,
					ProductTagId = ctx.Filter.ProductTagId ?? 0,
					IdMin = ctx.Filter.IdMinimum ?? 0,
					IdMax = ctx.Filter.IdMaximum ?? 0,
					AvailabilityMinimum = ctx.Filter.AvailabilityMinimum,
					AvailabilityMaximum = ctx.Filter.AvailabilityMaximum
				};

				if (!ctx.Filter.IsPublished.HasValue)
					searchContext.ShowHidden = true;

				if (ctx.Filter.CategoryIds != null && ctx.Filter.CategoryIds.Length > 0)
					searchContext.CategoryIds = ctx.Filter.CategoryIds.ToList();

				if (ctx.Filter.CreatedFrom.HasValue)
					searchContext.CreatedFromUtc = _dateTimeHelper.ConvertToUtcTime(ctx.Filter.CreatedFrom.Value, _dateTimeHelper.CurrentTimeZone);

				if (ctx.Filter.CreatedTo.HasValue)
					searchContext.CreatedToUtc = _dateTimeHelper.ConvertToUtcTime(ctx.Filter.CreatedTo.Value, _dateTimeHelper.CurrentTimeZone);

				query = _productService.PrepareProductSearchQuery(searchContext);

				query = query.OrderByDescending(x => x.CreatedOnUtc);
			}
			else
			{
				query = ctx.QueryProducts;
			}

			if (skip > 0)
				query = query.Skip(skip);

			if (take != int.MaxValue)
				query = query.Take(take);

			return query;
		}

		private List<Product> GetProducts(DataExportTaskContext ctx, int skip)
		{
			var result = new List<Product>();

			var products = GetProductQuery(ctx, skip, PageSize).ToList();

			foreach (var product in products)
			{
				if (product.ProductType == ProductType.SimpleProduct || product.ProductType == ProductType.BundledProduct)
				{
					result.Add(product);
				}
				else if (product.ProductType == ProductType.GroupedProduct)
				{
					if (ctx.Projection.NoGroupedProducts && !ctx.IsPreview)
					{
						var associatedSearchContext = new ProductSearchContext
						{
							OrderBy = ProductSortingEnum.CreatedOn,
							PageSize = int.MaxValue,
							StoreId = (ctx.Profile.PerStore ? ctx.Store.Id : ctx.Filter.StoreId),
							VisibleIndividuallyOnly = false,
							ParentGroupedProductId = product.Id
						};

						foreach (var associatedProduct in _productService.SearchProducts(associatedSearchContext))
						{
							result.Add(associatedProduct);
						}
					}
					else
					{
						result.Add(product);
					}
				}
			}

			try
			{
				SetProgress(ctx, products.Count);

				_services.DbContext.DetachEntities(result);
			}
			catch { }

			return result;
		}

		#endregion

		/// <summary>
		/// The name of the public export folder
		/// </summary>
		public static string PublicFolder
		{
			get { return "Exchange"; }
		}

		public static int PageSize
		{
			get { return 100; }
		}

		public DataExportResult Export(DataExportRequest request, CancellationToken cancellationToken)
		{
			return null;
		}

		public IList<dynamic> Preview(DataExportRequest request)
		{
			return null;
		}

		public long GetDataCount(DataExportRequest request)
		{
			return 0;
		}
	}
}
