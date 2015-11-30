using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Autofac;
using SmartStore.Core;
using SmartStore.Core.Async;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Domain.Tasks;
using SmartStore.Core.Html;
using SmartStore.Core.Logging;
using SmartStore.Services.Catalog;
using SmartStore.Services.Directory;
using SmartStore.Services.Localization;
using SmartStore.Services.Media;
using SmartStore.Services.Seo;
using SmartStore.Services.Stores;
using SmartStore.Services.Tasks;
using SmartStore.Services.Tax;
using SmartStore.Utilities;

namespace SmartStore.Web.Framework.Plugins
{
	public partial class FeedPluginHelper : PluginHelper
	{
		private readonly string _namespace;
		private ScheduleTask _scheduleTask;
		private IDictionary<int, Category> _cachedCategories;
		private IDictionary<int, string> _cachedPathes;
		private Func<PromotionFeedSettings> _settingsFunc;

		public FeedPluginHelper(IComponentContext componentContext, string systemName, string rootNamespace, Func<PromotionFeedSettings> settings, string providerResRootKey = null /* Legacy */) :
			base(componentContext, systemName, providerResRootKey)
		{
			_namespace = rootNamespace;
			_settingsFunc = settings;
		}

		#region Dependencies

		private IScheduleTaskService _scheduleTaskService;
		private IScheduleTaskService ScheduleTaskService
		{
			get { return _scheduleTaskService ?? (_scheduleTaskService = _ctx.Resolve<IScheduleTaskService>()); }
		}

		private IPictureService _pictureService;
		private IPictureService PictureService
		{
			get { return _pictureService ?? (_pictureService = _ctx.Resolve<IPictureService>()); }
		}

		private IProductService _productService;
		private IProductService ProductService
		{
			get { return _productService ?? (_productService = _ctx.Resolve<IProductService>()); }
		}

		private ICategoryService _categoryService;
		private ICategoryService CategoryService
		{
			get { return _categoryService ?? (_categoryService = _ctx.Resolve<ICategoryService>()); }
		}

		private INotifier _notifier;
		private INotifier Notifier
		{
			get { return _notifier ?? (_notifier = _ctx.Resolve<INotifier>()); }
		}

		private ILogger _logger;
		private ILogger Logger
		{
			get { return _logger ?? (_logger = _ctx.Resolve<ILogger>()); }
		}

		private IUrlRecordService _urlRecordService;
		private IUrlRecordService UrlRecordService
		{
			get { return _urlRecordService ?? (_urlRecordService = _ctx.Resolve<IUrlRecordService>()); }
		}

		private ILanguageService _languageService;
		private ILanguageService LanguageService
		{
			get { return _languageService ?? (_languageService = _ctx.Resolve<ILanguageService>()); }
		}

		private IPriceCalculationService _priceCalculationService;
		private IPriceCalculationService PriceCalculationService
		{
			get { return _priceCalculationService ?? (_priceCalculationService = _ctx.Resolve<IPriceCalculationService>()); }
		}

		private ITaxService _taxService;
		private ITaxService TaxService
		{
			get { return _taxService ?? (_taxService = _ctx.Resolve<ITaxService>()); }
		}

		private IWorkContext _workContext;
		private IWorkContext WorkContext
		{
			get { return _workContext ?? (_workContext = _ctx.Resolve<IWorkContext>()); }
		}

		private ICurrencyService _currencyService;
		private ICurrencyService CurrencyService
		{
			get { return _currencyService ?? (_currencyService = _ctx.Resolve<ICurrencyService>()); }
		}

		#endregion

		private PromotionFeedSettings BaseSettings 
		{ 
			get { return _settingsFunc(); } 
		}

		private string ScheduleTaskType
		{
			get
			{
				return "{0}.StaticFileGenerationTask, {0}".FormatWith(_namespace);
			}
		}

		public ScheduleTask ScheduleTask
		{
			get
			{
				if (_scheduleTask == null)
					_scheduleTask = ScheduleTaskService.GetTaskByType(ScheduleTaskType);

				return _scheduleTask;
			}
		}

		public string RemoveInvalidFeedChars(string input, bool isHtmlEncoded)
		{
			if (String.IsNullOrWhiteSpace(input))
				return input;

			//Microsoft uses a proprietary encoding (called CP-1252) for the bullet symbol and some other special characters, 
			//whereas most websites and data feeds use UTF-8. When you copy-paste from a Microsoft product into a website, 
			//some characters may appear as junk. Our system generates data feeds in the UTF-8 character encoding, 
			//which many shopping engines now require.

			//http://www.atensoftware.com/p90.php?q=182

			if (isHtmlEncoded)
				input = HttpUtility.HtmlDecode(input);

			input = input.Replace("¼", "");
			input = input.Replace("½", "");
			input = input.Replace("¾", "");

			if (isHtmlEncoded)
				input = HttpUtility.HtmlEncode(input);

			return input;
		}

		public string BuildProductDescription(string productName, string shortDescription, string fullDescription, string manufacturer, Func<string, string> updateResult = null)
		{
			if (BaseSettings.BuildDescription.IsCaseInsensitiveEqual(NotSpecified))
				return "";

			string description = "";

			if (BaseSettings.BuildDescription.IsEmpty())
			{
				description = fullDescription;

				if (description.IsEmpty())
					description = shortDescription;
				if (description.IsEmpty())
					description = productName;
			}
			else if (BaseSettings.BuildDescription.IsCaseInsensitiveEqual("short"))
			{
				description = shortDescription;
			}
			else if (BaseSettings.BuildDescription.IsCaseInsensitiveEqual("long"))
			{
				description = fullDescription;
			}
			else if (BaseSettings.BuildDescription.IsCaseInsensitiveEqual("titleAndShort"))
			{
				description = productName.Grow(shortDescription, " ");
			}
			else if (BaseSettings.BuildDescription.IsCaseInsensitiveEqual("titleAndLong"))
			{
				description = productName.Grow(fullDescription, " ");
			}
			else if (BaseSettings.BuildDescription.IsCaseInsensitiveEqual("manuAndTitleAndShort"))
			{
				description = manufacturer.Grow(productName, " ");
				description = description.Grow(shortDescription, " ");
			}
			else if (BaseSettings.BuildDescription.IsCaseInsensitiveEqual("manuAndTitleAndLong"))
			{
				description = manufacturer.Grow(productName, " ");
				description = description.Grow(fullDescription, " ");
			}

			if (updateResult != null)
			{
				description = updateResult(description);
			}

			try
			{
				if (BaseSettings.DescriptionToPlainText)
				{
					//Regex reg = new Regex("<[^>]+>", RegexOptions.IgnoreCase);
					//description = HttpUtility.HtmlDecode(reg.Replace(description, ""));

					description = HtmlUtils.ConvertHtmlToPlainText(description);
					description = HtmlUtils.StripTags(HttpUtility.HtmlDecode(description));

					description = RemoveInvalidFeedChars(description, false);
				}
				else
				{
					description = RemoveInvalidFeedChars(description, true);
				}
			}
			catch (Exception exc)
			{
				exc.Dump();
			}

			return description;
		}

		public string GetManufacturerPartNumber(Product product)
		{
			if (product.ManufacturerPartNumber.HasValue())
				return product.ManufacturerPartNumber;

			if (BaseSettings.UseOwnProductNo)
				return product.Sku;

			return "";
		}

		public string GetShippingCost(Product product, decimal productPrice, decimal? shippingCost = null)
		{
			if (product.IsFreeShipping)
				return "0";

			if (BaseSettings.FreeShippingThreshold.HasValue && productPrice >= BaseSettings.FreeShippingThreshold.Value)
				return "0";

			decimal cost = shippingCost ?? BaseSettings.ShippingCost;

			if (decimal.Compare(cost, decimal.Zero) == 0)
				return "";

			return cost.FormatInvariant();
		}

		public string GetProductDetailUrl(Store store, Product product)
		{
			return "{0}{1}".FormatWith(store.Url, product.GetSeName(Language.Id, UrlRecordService, LanguageService));
		}

		public decimal GetProductPrice(Product product, Currency currency, Store store)
		{
			decimal priceBase = PriceCalculationService.GetPreselectedPrice(product, null);

			if (BaseSettings.ConvertNetToGrossPrices)
			{
				decimal taxRate;
				priceBase = TaxService.GetProductPrice(product, priceBase, true, WorkContext.CurrentCustomer, out taxRate);
			}
			
			decimal price = CurrencyService.ConvertFromPrimaryStoreCurrency(priceBase, currency, store);
			return price;
		}

		public decimal? GetOldPrice(Product product, Currency currency, Store store)
		{
			if (!decimal.Equals(product.OldPrice, decimal.Zero) && !decimal.Equals(product.OldPrice, product.Price) &&
				!(product.ProductType == ProductType.BundledProduct && product.BundlePerItemPricing))
			{
				decimal price = product.OldPrice;

				if (BaseSettings.ConvertNetToGrossPrices)
				{
					decimal taxRate;
					price = TaxService.GetProductPrice(product, price, true, WorkContext.CurrentCustomer, out taxRate);
				}

				return CurrencyService.ConvertFromPrimaryStoreCurrency(price, currency, store);
			}
			return null;
		}
		
		public FeedFileData GetFeedFileByStore(Store store, string secondFileName = null, string extension = null)
		{
			if (store == null)
				return null;

			string dirTemp = FileSystemHelper.TempDir();
			string ext = extension ?? BaseSettings.ExportFormat;
			string dir = Path.Combine(HttpRuntime.AppDomainAppPath, "Content\\files\\exportimport");
			string fileName = "{0}_{1}".FormatWith(store.Id, BaseSettings.StaticFileName);
			string logName = Path.GetFileNameWithoutExtension(fileName) + ".txt";
			
			if (ext.HasValue())
				fileName = Path.GetFileNameWithoutExtension(fileName) + (ext.StartsWith(".") ? "" : ".") + ext;

			string url = "{0}content/files/exportimport/".FormatWith(store.Url.EnsureEndsWith("/"));

			if (!(url.StartsWith("http://") || url.StartsWith("https://")))
				url = "http://" + url;

			if (!Directory.Exists(dir))
				Directory.CreateDirectory(dir);

			var feedFile = new FeedFileData
			{
				StoreId = store.Id,
				StoreName = store.Name,
				FileTempPath = Path.Combine(dirTemp, fileName),
				FilePath = Path.Combine(dir, fileName),
				FileUrl = url + fileName,
				LogPath = Path.Combine(dir, logName),
				LogUrl = url + logName
			};

			try
			{
				feedFile.LastWriteTime = File.GetLastWriteTimeUtc(feedFile.FilePath).RelativeFormat(true, null);
			}
			catch (Exception)
			{
				feedFile.LastWriteTime = feedFile.LastWriteTime.NaIfEmpty();
			}

			if (secondFileName.HasValue())
			{
				string fname2 = store.Id + "_" + secondFileName;
				feedFile.CustomProperties.Add("SecondFileTempPath", Path.Combine(dirTemp, fname2));
				feedFile.CustomProperties.Add("SecondFilePath", Path.Combine(dir, fname2));
				feedFile.CustomProperties.Add("SecondFileUrl", url + fname2);
			}
			return feedFile;
		}

		public List<FeedFileData> GetFeedFiles(List<Store> stores, string secondFileName = null, string extension = null)
		{
			var lst = new List<FeedFileData>();

			foreach (var store in stores)
			{
				var feedFile = GetFeedFileByStore(store, secondFileName, extension);

				if (feedFile != null && File.Exists(feedFile.FilePath))
					lst.Add(feedFile);
			}
			return lst;
		}

		public void StartCreatingFeeds(Func<FeedFileCreationContext, bool> createFeed, string secondFileName = null)
		{
			try
			{
				using (var scope = new DbContextScope(autoDetectChanges: false, validateOnSave: false, forceNoTracking: true))
				{
					_cachedPathes = null;
					_cachedCategories = null;

					var storeService = _ctx.Resolve<IStoreService>();
					var stores = new List<Store>();

					if (BaseSettings.StoreId != 0)
					{
						var storeById = storeService.GetStoreById(BaseSettings.StoreId);
						if (storeById != null)
							stores.Add(storeById);
					}

					if (stores.Count == 0)
					{
						stores.AddRange(storeService.GetAllStores());
					}

					var context = new FeedFileCreationContext
					{
						StoreCount = stores.Count,
						Progress = new Progress<FeedFileCreationProgress>(x =>
						{
							AsyncState.Current.Set(x, SystemName);
						})
					};

					foreach (var store in stores)
					{
						var feedFile = GetFeedFileByStore(store, secondFileName);
						if (feedFile != null)
						{
							FileSystemHelper.Delete(feedFile.FileTempPath);

							if (secondFileName.HasValue())
								FileSystemHelper.Delete(feedFile.CustomProperties["SecondFileTempPath"] as string);

							using (var stream = new FileStream(feedFile.FileTempPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
							using (var logger = new TraceLogger(feedFile.LogPath))
							{
								context.Stream = stream;
								context.Logger = logger;
								context.Store = store;
								context.FeedFileUrl = feedFile.FileUrl;

								if (secondFileName.HasValue())
									context.SecondFilePath = feedFile.CustomProperties["SecondFileTempPath"] as string;

								if (!createFeed(context))
									break;
							}

							FileSystemHelper.Copy(feedFile.FileTempPath, feedFile.FilePath);

							if (secondFileName.HasValue())
								FileSystemHelper.Copy(context.SecondFilePath, feedFile.CustomProperties["SecondFilePath"] as string);
						}
					}
				}
			}
			finally
			{
				AsyncState.Current.Remove<FeedFileCreationProgress>(SystemName);
			}
		}

		public void DeleteFeedFiles(string secondFileName = null)
		{
			try
			{
				var extensions = new List<string>() { "xml", "csv" };

				if (!extensions.Contains(BaseSettings.ExportFormat))
					extensions.Add(BaseSettings.ExportFormat);

				var storeService = _ctx.Resolve<IStoreService>();
				var stores = storeService.GetAllStores();

				foreach (var store in stores)
				{
					foreach (var ext in extensions)
					{
						var feedFile = GetFeedFileByStore(store, secondFileName, ext);
						if (feedFile != null)
						{
							FileSystemHelper.Delete(feedFile.FileTempPath);
							FileSystemHelper.Delete(feedFile.FilePath);
							FileSystemHelper.Delete(feedFile.LogPath);

							if (secondFileName.HasValue())
							{
								FileSystemHelper.Delete(feedFile.CustomProperties["SecondFilePath"] as string);
								FileSystemHelper.Delete(feedFile.CustomProperties["SecondFileTempPath"] as string);
							}
						}
					}
				}
			}
			catch (Exception exc)
			{
				Notifier.Error(exc.Message);
			}
		}

		public string GetMainProductImageUrl(Store store, Product product)
		{
			string url;
			var pictureService = PictureService;
			var picture = product.GetDefaultProductPicture(pictureService);

			// always use HTTP when getting image URL
			if (picture != null)
				url = pictureService.GetPictureUrl(picture, BaseSettings.ProductPictureSize, storeLocation: store.Url);
			else
				url = pictureService.GetDefaultPictureUrl(BaseSettings.ProductPictureSize, storeLocation: store.Url);

			return url;
		}

		public List<string> GetAdditionalProductImages(Store store, Product product, string mainImageUrl, int maxImages = 10)
		{
			var urls = new List<string>();

			if (BaseSettings.AdditionalImages)
			{
				var pictureService = PictureService;
				var pics = pictureService.GetPicturesByProductId(product.Id, 0);

				foreach (var pic in pics)
				{
					if (pic != null)
					{
						string url = pictureService.GetPictureUrl(pic, BaseSettings.ProductPictureSize, storeLocation: store.Url);

						if (url.HasValue() && (mainImageUrl.IsEmpty() || !mainImageUrl.IsCaseInsensitiveEqual(url)))
						{
							urls.Add(url);
							if (urls.Count >= maxImages)
								break;
						}
					}
				}
			}
			return urls;
		}

		internal string LookupCategoryPath(int id)
		{
			if (_cachedPathes.ContainsKey(id))
			{
				return _cachedPathes[id];
			}
			return null;
		}

		internal void AddPathToCache(int id, string value)
		{
			_cachedPathes[id] = value;
		}

		internal Category LookupCategory(int id)
		{
			if (_cachedCategories.ContainsKey(id))
			{
				return _cachedCategories[id];
			}
			return null;
		}

		public string GetCategoryPath(Product product)
		{
			var categoryService = CategoryService;

			if (_cachedPathes == null)
			{
				_cachedPathes = new Dictionary<int, string>();
			}

			if (_cachedCategories == null)
			{
				var allCategories = categoryService.GetAllCategories(showHidden: true, applyNavigationFilters: false);
				_cachedCategories = allCategories.ToDictionary(x => x.Id);
			}

			string path = categoryService.GetCategoryPath(product, null, LookupCategoryPath, AddPathToCache, (i) => LookupCategory(i) ?? categoryService.GetCategoryById(i));

			return path;
		}

		public void UpdateScheduleTask(bool enabled, string cronExpression = "0 */6 * * *" /* every 6 hrs */)
		{
			var task = ScheduleTask;
			if (task != null)
			{
				task.Enabled = enabled;
				task.CronExpression = cronExpression;

				ScheduleTaskService.UpdateTask(task);
			}
		}

		public void InsertScheduleTask(string cronExpression = "0 */6 * * *" /* every 6 hrs */)
		{
			var task = ScheduleTask;
			if (task == null)
			{
				ScheduleTaskService.InsertTask(new ScheduleTask 
				{
					Name = "{0} feed file generation".FormatWith(SystemName),
					CronExpression = cronExpression,
					Type = ScheduleTaskType,
					Enabled = false,
					StopOnError = false,
				});
			}
		}

		public void DeleteScheduleTask()
		{
			var task = ScheduleTask;
			if (task != null)
				ScheduleTaskService.DeleteTask(task);
		}

		public bool RunScheduleTask()
		{
			if (ScheduleTask.IsRunning)
			{
				Notifier.Information(GetProgressInfo());
				return true;
			}

            var taskScheduler = _ctx.Resolve<ITaskScheduler>();
			taskScheduler.RunSingleTask(ScheduleTask.Id);

            Notifier.Information(GetResource("Admin.System.ScheduleTasks.RunNow.Progress"));

            return true;
		}

		public string GetProgressInfo(bool checkIfRunnning = false)
		{
			if (checkIfRunnning)
			{
				var task = ScheduleTask;
				if (task != null && !task.IsRunning)
					return "";
			}

			string result = GetResource("Admin.System.ScheduleTasks.RunNow.Progress");

			try
			{
				var progress = AsyncState.Current.Get<FeedFileCreationProgress>(SystemName);

				if (progress != null)
				{
					int percent = (int)progress.ProcessedPercent;

					if (percent > 0 && percent <= 100)
						result = "{0}... {1}%".FormatWith(result, percent);
				}
			}
			catch (Exception) { }

			return result;
		}

		public void SetupConfigModel(PromotionFeedConfigModel model, string controller, string secondFileName = null, string[] extensions = null)
		{
			var storeService = _ctx.Resolve<IStoreService>();
			var stores = storeService.GetAllStores().ToList();

			var urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);
			var routeValues = new { /*Namespaces = _namespace + ".Controllers",*/ area = _namespace };

			model.GenerateFeedUrl = urlHelper.Action("GenerateFeed", controller, routeValues);
			model.GenerateFeedProgressUrl = urlHelper.Action("GenerateFeedProgress", controller, routeValues);
			model.DeleteFilesUrl = urlHelper.Action("DeleteFiles", controller, routeValues);

			model.Helper = this;
			model.AvailableStores = new List<SelectListItem>();
			model.AvailableStores.Add(new SelectListItem() { Text = GetResource("Admin.Common.All"), Value = "0" });
			model.AvailableStores.AddRange(stores.ToSelectListItems());

			model.AvailableLanguages = new List<SelectListItem>();
			model.AvailableLanguages.Add(new SelectListItem() { Text = GetResource("Admin.Common.Standard"), Value = "0" });

			foreach (var language in LanguageService.GetAllLanguages())
			{
				model.AvailableLanguages.Add(new SelectListItem()
				{
					Text = language.Name,
					Value = language.Id.ToString()
				});
			}

			if (!model.IsRunning)
				model.IsRunning = (ScheduleTask != null && ScheduleTask.IsRunning);

			if (model.IsRunning)
			{
				model.ProcessInfo = GetProgressInfo();
			}
			else
			{
				if (extensions != null)
				{
					foreach (var ext in extensions)
						model.GeneratedFiles.AddRange(GetFeedFiles(stores, null, ext));
				}
				else
				{
					model.GeneratedFiles = GetFeedFiles(stores, secondFileName);
				}
			}
		}
	}
}
