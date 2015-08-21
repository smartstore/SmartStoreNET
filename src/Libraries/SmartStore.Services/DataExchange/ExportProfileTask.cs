using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using Autofac;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Domain;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Html;
using SmartStore.Core.Logging;
using SmartStore.Core.Plugins;
using SmartStore.Services.Catalog;
using SmartStore.Services.Customers;
using SmartStore.Services.Directory;
using SmartStore.Services.Helpers;
using SmartStore.Services.Localization;
using SmartStore.Services.Media;
using SmartStore.Services.Tasks;
using SmartStore.Services.Tax;
using SmartStore.Utilities;

namespace SmartStore.Services.DataExchange
{
	public class ExportProfileTask : ITask
	{
		private const string _logName = "log.txt";
		private const int _maxErrors = 20;
		private const int _pageSize = 100;

		private ICommonServices _services;
		private IExportService _exportService;
		private IProductService _productService;
		private IPictureService _pictureService;
		private MediaSettings _mediaSettings;
		private IPriceCalculationService _priceCalculationService;
		private ITaxService _taxService;
		private ICurrencyService _currencyService;
		private ICustomerService _customerService;
		private ICategoryService _categoryService;
		private IPriceFormatter _priceFormatter;
		private IDateTimeHelper _dateTimeHelper;

		#region Utilities

		private void PrepareProductDescription(ExportProfileTaskContext ctx, dynamic expando)
		{
			try
			{
				string description = "";

				// description merging
				if (ctx.Projection.DescriptionMerging.HasValue)
				{
					var type = ctx.Projection.DescriptionMerging ?? ExportDescriptionMerging.ShortDescriptionOrNameIfEmpty;

					if (type == ExportDescriptionMerging.ShortDescriptionOrNameIfEmpty)
					{
						description = expando.FullDescription;

						if (description.IsEmpty())
							description = expando.ShortDescription;
						if (description.IsEmpty())
							description = expando.Name;
					}
					else if (type == ExportDescriptionMerging.ShortDescription)
					{
						description = expando.ShortDescription;
					}
					else if (type == ExportDescriptionMerging.Description)
					{
						description = expando.FullDescription;
					}
					else if (type == ExportDescriptionMerging.NameAndShortDescription)
					{
						description = ((string)expando.Name).Grow((string)expando.ShortDescription, " ");
					}
					else if (type == ExportDescriptionMerging.NameAndDescription)
					{
						description = ((string)expando.Name).Grow((string)expando.FullDescription, " ");
					}
					else if (type == ExportDescriptionMerging.ManufacturerAndNameAndShortDescription || type == ExportDescriptionMerging.ManufacturerAndNameAndDescription)
					{
						string name = (string)expando.Name;
						dynamic productManu = ((List<ExpandoObject>)expando.ProductManufacturers).FirstOrDefault();

						if (productManu != null)
						{
							dynamic manu = productManu.Manufacturer;
							description = ((string)manu.Name).Grow(name, " ");

							if (type == ExportDescriptionMerging.ManufacturerAndNameAndShortDescription)
								description = description.Grow((string)expando.ShortDescription, " ");
							else
								description = description.Grow((string)expando.FullDescription, " ");
						}
					}
				}
				else
				{
					description = expando.FullDescription;
				}

				// append text
				if (ctx.Projection.AppendDescriptionText.HasValue() && ((string)expando.ShortDescription).IsEmpty() && ((string)expando.FullDescription).IsEmpty())
				{
					string[] appendText = ctx.Projection.AppendDescriptionText.SplitSafe(";");
					if (appendText.Length > 0)
					{
						var rnd = (new Random()).Next(0, appendText.Length - 1);

						description = description.Grow(appendText.SafeGet(rnd), " ");
					}
				}

				// remove critical characters
				if (description.HasValue() && ctx.Projection.RemoveCriticalCharacters)
				{
					foreach (var str in ctx.Projection.CriticalCharacters.SplitSafe(";"))
						description = description.Replace(str, "");
				}

				// convert to plain text
				if (ctx.Projection.DescriptionToPlainText)
				{
					//Regex reg = new Regex("<[^>]+>", RegexOptions.IgnoreCase);
					//description = HttpUtility.HtmlDecode(reg.Replace(description, ""));

					description = HtmlUtils.ConvertHtmlToPlainText(description);
					description = HtmlUtils.StripTags(HttpUtility.HtmlDecode(description));
				}

				expando.FullDescription = description;
			}
			catch { }
		}

		private void PrepareProductPrice(ExportProfileTaskContext ctx, dynamic expando, Product product)
		{
			decimal price = decimal.Zero;

			// price type
			if (ctx.Projection.PriceType.HasValue)
			{
				var type = ctx.Projection.PriceType ?? PriceDisplayType.PreSelectedPrice;

				if (type == PriceDisplayType.LowestPrice)
				{
					bool displayFromMessage;
					price = _priceCalculationService.GetLowestPrice(product, null, out displayFromMessage);
				}
				else if (type == PriceDisplayType.PreSelectedPrice)
				{
					price = _priceCalculationService.GetPreselectedPrice(product, null);
				}
				else if (type == PriceDisplayType.PriceWithoutDiscountsAndAttributes)
				{
					price = _priceCalculationService.GetFinalPrice(product, null, ctx.ProjectionCustomer, decimal.Zero, false, 1, null, null);
				}
			}
			else
			{
				price = expando.Price;
			}

			// convert net to gross
			if (ctx.Projection.ConvertNetToGrossPrices)
			{
				decimal taxRate;
				price = _taxService.GetProductPrice(product, price, true, ctx.ProjectionCustomer, out taxRate);
			}

			if (price != decimal.Zero)
			{
				price = _currencyService.ConvertFromPrimaryStoreCurrency(price, ctx.ProjectionCurrency, ctx.Store);
			}

			expando.Price = price;
		}

		#endregion

		private void InitDependencies(TaskExecutionContext context)
		{
			_services = context.Resolve<ICommonServices>();
			_exportService = context.Resolve<IExportService>();
			_productService = context.Resolve<IProductService>();
			_pictureService = context.Resolve<IPictureService>();
			_mediaSettings = context.Resolve<MediaSettings>();
			_priceCalculationService = context.Resolve<IPriceCalculationService>();
			_taxService = context.Resolve<ITaxService>();
			_currencyService = context.Resolve<ICurrencyService>();
			_customerService = context.Resolve<ICustomerService>();
			_categoryService = context.Resolve<ICategoryService>();
			_priceFormatter = context.Resolve<IPriceFormatter>();
			_dateTimeHelper = context.Resolve<IDateTimeHelper>();
		}

		private IEnumerable<Product> GetProducts(ExportProfileTaskContext ctx, int pageIndex)
		{
			// do not call DetachAll() here! It will detach everything even things you do not want to have detached.
			// note that detached navigation properties will not navigate again. they stay null => exception.
			// DetachAll() is actually more useful to be called after all the work is done.

			//_services.DbContext.DetachAll();

			if (!ctx.Cancellation.IsCancellationRequested)
			{
				var searchContext = new ProductSearchContext
				{
					OrderBy = ProductSortingEnum.CreatedOn,
					PageIndex = pageIndex,
					PageSize = _pageSize,
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

				if (ctx.Filter.CategoryIds != null && ctx.Filter.CategoryIds.Length > 0)
					searchContext.CategoryIds = ctx.Filter.CategoryIds.ToList();

				if (ctx.Filter.CreatedFrom.HasValue)
					searchContext.CreatedFromUtc = _dateTimeHelper.ConvertToUtcTime(ctx.Filter.CreatedFrom.Value, _dateTimeHelper.CurrentTimeZone);

				if (ctx.Filter.CreatedTo.HasValue)
					searchContext.CreatedToUtc = _dateTimeHelper.ConvertToUtcTime(ctx.Filter.CreatedTo.Value, _dateTimeHelper.CurrentTimeZone);


				var products = _productService.SearchProducts(searchContext);

				foreach (var product in products)
				{
					if (product.ProductType == ProductType.SimpleProduct || product.ProductType == ProductType.BundledProduct)
					{
						yield return product;
					}
					else if (product.ProductType == ProductType.GroupedProduct)
					{
						var associatedSearchContext = new ProductSearchContext
						{
							OrderBy = ProductSortingEnum.CreatedOn,
							PageSize = int.MaxValue,
							StoreId = (ctx.Profile.PerStore ? ctx.Store.Id : 0),
							VisibleIndividuallyOnly = false,
							ParentGroupedProductId = product.Id
						};

						foreach (var associatedProduct in _productService.SearchProducts(associatedSearchContext))
						{
							yield return associatedProduct;
						}
					}
				}
			}
		}

		private ExpandoObject ToExpando(ExportProfileTaskContext ctx, Product product)
		{
			dynamic expando = product.ToExpando(ctx.Projection.LanguageId ?? 0);

			PrepareProductPrice(ctx, expando, product);

			expando._BasePriceInfo = product.GetBasePriceInfo(_services.Localization, _priceFormatter, decimal.Zero, true);

			if (ctx.Provider.Supports(ExportProjectionSupport.Description))
			{
				PrepareProductDescription(ctx, expando);
			}

			if (ctx.Provider.Supports(ExportProjectionSupport.Brand))
			{
				string brand = null;
				var manu = product.ProductManufacturers.OrderBy(x => x.DisplayOrder).FirstOrDefault();

				if (manu != null)
					brand = manu.Manufacturer.GetLocalized(x => x.Name, ctx.Projection.LanguageId ?? 0, true, false);

				if (brand.IsEmpty())
					brand = ctx.Projection.Brand;

				expando._Brand = brand;
			}

			if (ctx.Provider.Supports(ExportProjectionSupport.UseOwnProductNo) && product.ManufacturerPartNumber.IsEmpty())
			{
				expando.ManufacturerPartNumber = product.Sku;
			}

			if (ctx.Provider.Supports(ExportProjectionSupport.CategoryPath))
			{
				if (ctx.CategoryPathes == null)
				{
					ctx.CategoryPathes = new Dictionary<int, string>();
				}

				if (ctx.Categories == null)
				{
					var allCategories = _categoryService.GetAllCategories(showHidden: true, applyNavigationFilters: false);
					ctx.Categories = allCategories.ToDictionary(x => x.Id);
				}

				expando._CategoryPath = _categoryService.GetCategoryPath(
					product,
					null,
					x => ctx.CategoryPathes.ContainsKey(x) ? ctx.CategoryPathes[x] : null,
					(id, value) => ctx.CategoryPathes[id] = value,
					x => ctx.Categories.ContainsKey(x) ? ctx.Categories[x] : _categoryService.GetCategoryById(x)
				);
			}

			if (ctx.Provider.Supports(ExportProjectionSupport.MainPictureUrl))
			{
				var picture = product.GetDefaultProductPicture(_pictureService);

				// always use HTTP when getting image URL
				if (picture != null)
					expando._MainPictureUrl = _pictureService.GetPictureUrl(picture, ctx.Projection.PictureSize, storeLocation: ctx.Store.Url);
				else
					expando._MainPictureUrl = _pictureService.GetDefaultPictureUrl(ctx.Projection.PictureSize, storeLocation: ctx.Store.Url);
			}

			if (ctx.Provider.Supports(ExportProjectionSupport.ShippingTime))
			{
				dynamic deliveryTime = expando.DeliveryTime;
				expando._ShippingTime = (deliveryTime == null ? ctx.Projection.ShippingTime : deliveryTime.Name);
			}

			if (ctx.Provider.Supports(ExportProjectionSupport.ShippingCosts))
			{
				expando._FreeShippingThreshold = ctx.Projection.FreeShippingThreshold;

				if (product.IsFreeShipping || (ctx.Projection.FreeShippingThreshold.HasValue && (decimal)expando.Price >= ctx.Projection.FreeShippingThreshold.Value))
					expando._ShippingCosts = decimal.Zero;
				else
					expando._ShippingCosts = ctx.Projection.ShippingCosts;
			}

			return expando as ExpandoObject;
		}

		private void Cleanup(ExportProfileTaskContext ctx)
		{
			FileSystemHelper.ClearDirectory(ctx.Folder, false, new List<string> { _logName });
			
			// TODO: more deployment specific here
		}

		private void ExportCoreInner(ExportProfileTaskContext ctx)
		{
			ctx.Export.StoreId = ctx.Store.Id;
			ctx.Export.StoreUrl = ctx.Store.Url;

			// be careful with too long file system paths
			ctx.Export.FileNamePattern = string.Concat(
				"{0}-",
				ctx.Profile.PerStore ? SeoHelper.GetSeName(ctx.Store.Name, true, false).ToValidFileName("").Truncate(20) : "all-stores",
				"{1}",
				ctx.Provider.Value.FileExtension.ToLower().EnsureStartsWith(".")
			);

			{
				var logHead = new StringBuilder();
				logHead.AppendLine();
				logHead.AppendLine(new string('-', 40));
				logHead.AppendLine("SmartStore.NET:\t\tv." + SmartStoreVersion.CurrentFullVersion);
				logHead.AppendLine("Export profile:\t\t{0} (Id {1})".FormatInvariant(ctx.Profile.Name, ctx.Profile.Id));

				var plugin = ctx.Provider.Metadata.PluginDescriptor;
				logHead.Append("Plugin:\t\t\t\t");
				logHead.AppendLine(plugin == null ? "".NaIfEmpty() : "{0} ({1}) v.{2}".FormatInvariant(plugin.FriendlyName, plugin.SystemName, plugin.Version.ToString()));

				logHead.AppendLine("Export provider:\t{0} ({1})".FormatInvariant(ctx.Provider == null ? "".NaIfEmpty() : ctx.Provider.Metadata.FriendlyName, ctx.Profile.ProviderSystemName));

				var storeInfo = (ctx.Profile.PerStore ? "{0} (Id {1})".FormatInvariant(ctx.Store.Name, ctx.Store.Id) : "all stores");
				logHead.Append("Store:\t\t\t\t" + storeInfo);

				ctx.Log.Information(logHead.ToString());
			}

			if (ctx.Provider == null)
			{
				throw new SmartException("Export aborted because the export provider cannot be loaded");
			}

			if (!ctx.Provider.IsValid())
			{
				throw new SmartException("Export aborted because the export provider is not valid");
			}

			if (ctx.Provider.Value.EntityType == ExportEntityType.Product)
			{
				var anySingleProduct = _productService.SearchProducts(new ProductSearchContext
				{
					OrderBy = ProductSortingEnum.CreatedOn,
					PageIndex = ctx.Profile.Offset,
					PageSize = 1,
					StoreId = (ctx.Profile.PerStore ? ctx.Store.Id : 0),
					VisibleIndividuallyOnly = true
				});

				ctx.Segmenter = new ExportSegmenter(
					pageIndex => GetProducts(ctx, pageIndex),
					entity => ToExpando(ctx, (Product)entity),
					new PagedList(ctx.Profile.Offset, ctx.Profile.Limit, _pageSize, anySingleProduct.TotalCount),
					ctx.Profile.BatchSize
				);
			}


			if (ctx.Segmenter == null)
			{
				throw new SmartException("Unsupported entity type '{0}'".FormatInvariant(ctx.Provider.Value.EntityType.ToString()));
			}
			else
			{
				ctx.Export.Data = ctx.Segmenter;

				ctx.Segmenter.Start(() =>
				{
					if (!ctx.Provider.Value.Execute(ctx.Export))
						return false;

					return !ctx.Cancellation.IsCancellationRequested;
				});
			}
		}

		private void ExportCoreOuter(ExportProfileTaskContext ctx)
		{
			if (ctx.Profile == null || !ctx.Profile.Enabled)
				return;

			try
			{
				FileSystemHelper.ClearDirectory(ctx.Folder, false);

				var allStores = _services.StoreService.GetAllStores();
				var currentStore = _services.StoreContext.CurrentStore;

				using (var logger = new TraceLogger(Path.Combine(ctx.Folder, _logName)))
				{
					ctx.Log = logger;
					ctx.Export.Log = logger;

					if (ctx.Projection.CurrencyId.HasValue)
						ctx.ProjectionCurrency = _currencyService.GetCurrencyById(ctx.Projection.CurrencyId.Value);
					else
						ctx.ProjectionCurrency = _services.WorkContext.WorkingCurrency;

					if (ctx.Projection.CustomerId.HasValue)
						ctx.ProjectionCustomer = _customerService.GetCustomerById(ctx.Projection.CustomerId.Value);
					else
						ctx.ProjectionCustomer = _services.WorkContext.CurrentCustomer;

					if (ctx.Profile.ProviderConfigData.HasValue())
					{
						string partialName;
						Type dataType;
						if (ctx.Provider.Value.RequiresConfiguration(out partialName, out dataType))
						{
							ctx.Export.ConfigurationData = XmlHelper.Deserialize(ctx.Profile.ProviderConfigData, dataType);
						}
					}

					using (var scope = new DbContextScope(_services.DbContext, autoDetectChanges: false, proxyCreation: true, validateOnSave: false, forceNoTracking: true))
					{
						if (ctx.Profile.PerStore)
						{
							foreach (var store in allStores.Where(x => x.Id == ctx.Filter.StoreId || ctx.Filter.StoreId == 0))
							{
								ctx.Store = store;

								ExportCoreInner(ctx);
							}
						}
						else
						{
							if (ctx.Filter.StoreId == 0)
								ctx.Store = allStores.FirstOrDefault(x => x.Id == (ctx.Projection.StoreId ?? currentStore.Id));
							else
								ctx.Store = allStores.FirstOrDefault(x => x.Id == ctx.Filter.StoreId);

							ExportCoreInner(ctx);
						}
					}
				}
			}
			catch (Exception exc)
			{
				ctx.Log.Error(exc);
			}
			finally
			{
				try
				{
					if (ctx.Profile.Cleanup)
						Cleanup(ctx);
				}
				catch { }

				try
				{
					if (ctx.Cancellation.IsCancellationRequested)
						ctx.Log.Warning("Export aborted. A cancellation has been requested");

					// TODO: log number of flown out records

					if (ctx.Segmenter != null)
						ctx.Segmenter.Dispose();

					ctx.Export.CustomProperties.Clear();

					// do not call during processing!
					_services.DbContext.DetachAll();
				}
				catch { }
			}
		}


		public void Execute(TaskExecutionContext context)
		{
			InitDependencies(context);

			var profileId = context.ScheduleTask.Alias.ToInt();
			var profile = _exportService.GetExportProfileById(profileId);

			var ctx = new ExportProfileTaskContext(profile, _exportService.LoadProvider(profile.ProviderSystemName), context.CancellationToken);

			ExportCoreOuter(ctx);
		}

		// TODO: is method required?
		public void Execute(ExportProfile profile, IComponentContext context)
		{
			if (profile == null)
				throw new ArgumentNullException("profile");

			if (context == null)
				throw new ArgumentNullException("context");

			InitDependencies(new TaskExecutionContext(context, null));

			var cancellation = new CancellationTokenSource(TimeSpan.FromHours(4.0));

			var ctx = new ExportProfileTaskContext(profile, _exportService.LoadProvider(profile.ProviderSystemName), cancellation.Token);

			ExportCoreOuter(ctx);
		}
	}


	internal class ExportProfileTaskContext
	{
		public ExportProfileTaskContext(ExportProfile profile, Provider<IExportProvider> provider, CancellationToken cancellation)
		{
			Debug.Assert(profile.FolderName.HasValue(), "Folder name must not be empty.");

			Profile = profile;
			Provider = provider;
			Filter = XmlHelper.Deserialize<ExportFilter>(profile.Filtering);
			Projection = XmlHelper.Deserialize<ExportProjection>(profile.Projection);
			Cancellation = cancellation;
			Folder = FileSystemHelper.TempDir(@"Profile\Export\{0}".FormatInvariant(profile.FolderName));

			Export = new ExportExecuteContext(Cancellation, Folder);
		}

		public ExportProfile Profile { get; private set; }
		public Provider<IExportProvider> Provider { get; private set; }
		public ExportFilter Filter { get; private set; }

		public ExportProjection Projection { get; private set; }
		public Currency ProjectionCurrency { get; set; }
		public Customer ProjectionCustomer { get; set; }

		public CancellationToken Cancellation { get; private set; }
		public TraceLogger Log { get; set; }
		public Store Store { get; set; }

		public string Folder { get; private set; }

		public Dictionary<int, Category> Categories;
		public Dictionary<int, string> CategoryPathes;

		public ExportSegmenter Segmenter { get; set; }

		public ExportExecuteContext Export { get; set; }
	}
}
