using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
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
using SmartStore.Core.Domain.Messages;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Email;
using SmartStore.Core.Html;
using SmartStore.Core.IO;
using SmartStore.Core.Logging;
using SmartStore.Core.Plugins;
using SmartStore.Services.Catalog;
using SmartStore.Services.Customers;
using SmartStore.Services.Directory;
using SmartStore.Services.Helpers;
using SmartStore.Services.Localization;
using SmartStore.Services.Media;
using SmartStore.Services.Messages;
using SmartStore.Services.Tasks;
using SmartStore.Services.Tax;
using SmartStore.Utilities;

namespace SmartStore.Services.DataExchange
{
	public class ExportProfileTask : ITask
	{
		private const int _maxErrors = 20;
		private const int _pageSize = 100;
		// TODO: replaces Content\files\ExportImport... add it to CommonController.RobotsTextFile, CommonController.MaintenanceDeleteFiles, FilePermissionHelper.GetDirectoriesWrite
		private const string _publicFolder = "Exchange";

		#region Dependencies

		private DataExchangeSettings _dataExchangeSettings;
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
		private IEmailAccountService _emailAccountService;
		private IEmailSender _emailSender;
		private IQueuedEmailService _queuedEmailService;
		private IProductAttributeService _productAttributeService;
		private IDeliveryTimeService _deliveryTimeService;
		private IQuantityUnitService _quantityUnitService;
		private IManufacturerService _manufacturerService;

		private void InitDependencies(TaskExecutionContext context)
		{
			_dataExchangeSettings = context.Resolve<DataExchangeSettings>();
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
			_emailAccountService = context.Resolve<IEmailAccountService>();
			_emailSender = context.Resolve<IEmailSender>();
			_queuedEmailService = context.Resolve<IQueuedEmailService>();
			_productAttributeService = context.Resolve<IProductAttributeService>();
			_deliveryTimeService = context.Resolve<IDeliveryTimeService>();
			_quantityUnitService = context.Resolve<IQuantityUnitService>();
			_manufacturerService = context.Resolve<IManufacturerService>();
		}

		#endregion

		#region Utilities

		private void PrepareProductDescription(ExportProfileTaskContext ctx, dynamic expando, Product product)
		{
			try
			{
				var languageId = (ctx.Projection.LanguageId ?? 0);
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
						var productManus = ctx.DataContext.ProductManufacturers.Load(product.Id);

						if (productManus != null && productManus.Any())
							description = productManus.First().Manufacturer.GetLocalized(x => x.Name, languageId, true, false);

						description = description.Grow((string)expando.Name, " ");

						if (type == ExportDescriptionMerging.ManufacturerAndNameAndShortDescription)
							description = description.Grow((string)expando.ShortDescription, " ");
						else
							description = description.Grow((string)expando.FullDescription, " ");
					}
				}
				else
				{
					description = expando.FullDescription;
				}

				// append text
				if (ctx.Projection.AppendDescriptionText.HasValue() && ((string)expando.ShortDescription).IsEmpty() && ((string)expando.FullDescription).IsEmpty())
				{
					string[] appendText = ctx.Projection.AppendDescriptionText.SplitSafe(",");
					if (appendText.Length > 0)
					{
						var rnd = (new Random()).Next(0, appendText.Length - 1);

						description = description.Grow(appendText.SafeGet(rnd), " ");
					}
				}

				// remove critical characters
				if (description.HasValue() && ctx.Projection.RemoveCriticalCharacters)
				{
					foreach (var str in ctx.Projection.CriticalCharacters.SplitSafe(","))
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
			var priceCalculationContext = ctx.DataContext as PriceCalculationContext;

			// price type
			if (ctx.Projection.PriceType.HasValue)
			{
				var type = ctx.Projection.PriceType ?? PriceDisplayType.PreSelectedPrice;

				if (type == PriceDisplayType.LowestPrice)
				{
					bool displayFromMessage;
					price = _priceCalculationService.GetLowestPrice(product, priceCalculationContext, out displayFromMessage);
				}
				else if (type == PriceDisplayType.PreSelectedPrice)
				{
					price = _priceCalculationService.GetPreselectedPrice(product, priceCalculationContext);
				}
				else if (type == PriceDisplayType.PriceWithoutDiscountsAndAttributes)
				{
					price = _priceCalculationService.GetFinalPrice(product, null, ctx.ProjectionCustomer, decimal.Zero, false, 1, null, priceCalculationContext);
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

		private void SendCompletionEmail(ExportProfileTaskContext ctx)
		{
			var emailAccount = _emailAccountService.GetEmailAccountById(ctx.Profile.EmailAccountId);
			var smtpContext = new SmtpContext(emailAccount);
			var message = new EmailMessage();

			var storeInfo = "{0} ({1})".FormatInvariant(ctx.Store.Name, ctx.Store.Url);

			message.To.AddRange(ctx.Profile.CompletedEmailAddresses.SplitSafe(",").Where(x => x.IsEmail()).Select(x => new EmailAddress(x)));
			message.From = new EmailAddress(emailAccount.Email, emailAccount.DisplayName);

			message.Subject = _services.Localization.GetResource("Admin.Configuration.Export.CompletedEmail.Subject", ctx.Projection.LanguageId ?? 0)
				.FormatInvariant(ctx.Profile.Name);

			message.Body = _services.Localization.GetResource("Admin.Configuration.Export.CompletedEmail.Body", ctx.Projection.LanguageId ?? 0)
				.FormatInvariant(storeInfo);

			_emailSender.SendEmail(smtpContext, message);
		}

		#endregion

		private IEnumerable<Product> GetProducts(ExportProfileTaskContext ctx, int pageIndex)
		{
			if (ctx.DataContext != null)
			{
				ctx.DataContext.Clear();
				ctx.DataContext = null;
			}

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

		private ExpandoObject ToExpando(ExportProfileTaskContext ctx, Product product, IEnumerable<Product> products)
		{
			// load data behind navigation properties for current page in one go
			if (ctx.DataContext == null)
			{
				ctx.DataContext = new ExportDataContext(products,
					x => _productAttributeService.GetProductVariantAttributesByProductIds(x, null),
					x => _productAttributeService.GetProductVariantAttributeCombinations(x),
					x => _productService.GetTierPrices(x, ctx.ProjectionCustomer, ctx.Store.Id),
					x => _categoryService.GetProductCategoriesByProductIds(x, true),
					x => _manufacturerService.GetProductManufacturersByProductIds(x),
					x => _productService.GetProductPicturesByProductIds(x)
				);
			}

			var languageId = (ctx.Projection.LanguageId ?? 0);
			var productPictures = ctx.DataContext.ProductPictures.Load(product.Id);
			var productManufacturers = ctx.DataContext.ProductManufacturers.Load(product.Id);
			var productCategories = ctx.DataContext.ProductCategories.Load(product.Id);
			var productAttributes = ctx.DataContext.Attributes.Load(product.Id);

			dynamic expando = product.ToExpando(languageId);

			// general data
			PrepareProductPrice(ctx, expando, product);

			expando._BasePriceInfo = product.GetBasePriceInfo(_services.Localization, _priceFormatter, decimal.Zero, true);

			expando._CategoryPath = _categoryService.GetCategoryPath(
				product,
				null,
				x => ctx.CategoryPathes.ContainsKey(x) ? ctx.CategoryPathes[x] : null,
				(id, value) => ctx.CategoryPathes[id] = value,
				x => ctx.Categories.ContainsKey(x) ? ctx.Categories[x] : _categoryService.GetCategoryById(x)
			);


			// navigation properties
			if (product.DeliveryTimeId.HasValue && ctx.DeliveryTimes.ContainsKey(product.DeliveryTimeId.Value))
				expando.DeliveryTime = ctx.DeliveryTimes[product.DeliveryTimeId.Value].ToExpando(languageId);
			else
				expando.DeliveryTime = null;

			if (product.QuantityUnitId.HasValue && ctx.QuantityUnits.ContainsKey(product.QuantityUnitId.Value))
				expando.QuantityUnit = ctx.QuantityUnits[product.QuantityUnitId.Value].ToExpando(languageId);
			else
				expando.QuantityUnit = null;

			expando.ProductPictures = productPictures
				.OrderBy(x => x.DisplayOrder)
				.Select(x =>
				{
					dynamic exp = new ExpandoObject();
					exp._Entity = x;
					exp.Id = x.Id;
					exp.DisplayOrder = x.DisplayOrder;
					exp.Picture = x.Picture.ToExpando(_pictureService, ctx.Store, _mediaSettings.ProductThumbPictureSize, _mediaSettings.ProductDetailsPictureSize);

					return exp as ExpandoObject;
				})
				.ToList();

			expando.ProductManufacturers = productManufacturers
				.OrderBy(x => x.DisplayOrder)
				.Select(x =>
				{
					dynamic exp = new ExpandoObject();
					exp._Entity = x;
					exp.Id = x.Id;
					exp.DisplayOrder = x.DisplayOrder;
					exp.IsFeaturedProduct = x.IsFeaturedProduct;
					exp.Manufacturer = x.Manufacturer.ToExpando(languageId);

					return exp as ExpandoObject;
				})
				.ToList();

			expando.ProductCategories = productCategories
				.OrderBy(x => x.DisplayOrder)
				.Select(x =>
				{
					dynamic exp = new ExpandoObject();
					exp._Entity = x;
					exp.Id = x.Id;
					exp.DisplayOrder = x.DisplayOrder;
					exp.IsFeaturedProduct = x.IsFeaturedProduct;
					exp.Category = x.Category.ToExpando(languageId);

					return exp as ExpandoObject;
				})
				.ToList();

			expando.ProductAttributes = productAttributes
				.OrderBy(x => x.DisplayOrder)
				.Select(x => x.ToExpando(languageId))
				.ToList();


			// data controlled through ExportProjectionSupport attribute
			if (ctx.Provider.Supports(ExportProjectionSupport.Description))
			{
				PrepareProductDescription(ctx, expando, product);
			}

			if (ctx.Provider.Supports(ExportProjectionSupport.Brand))
			{
				string brand = null;
				var productManus = ctx.DataContext.ProductManufacturers.Load(product.Id);

				if (productManus != null && productManus.Any())
					brand = productManus.First().Manufacturer.GetLocalized(x => x.Name, languageId, true, false);

				if (brand.IsEmpty())
					brand = ctx.Projection.Brand;

				expando._Brand = brand;
			}

			if (ctx.Provider.Supports(ExportProjectionSupport.UseOwnProductNo) && product.ManufacturerPartNumber.IsEmpty())
			{
				expando.ManufacturerPartNumber = product.Sku;
			}

			if (ctx.Provider.Supports(ExportProjectionSupport.MainPictureUrl))
			{
				if (productPictures != null && productPictures.Any())
					expando._MainPictureUrl = _pictureService.GetPictureUrl(productPictures.First().Picture, ctx.Projection.PictureSize, storeLocation: ctx.Store.Url);
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

			if (ctx.Provider.Supports(ExportProjectionSupport.OldPrice))
			{
				if (product.OldPrice != decimal.Zero && product.OldPrice != product.Price && !(product.ProductType == ProductType.BundledProduct && product.BundlePerItemPricing))
				{
					if (ctx.Projection.ConvertNetToGrossPrices)
					{
						decimal taxRate;
						expando._OldPrice = _taxService.GetProductPrice(product, product.OldPrice, true, ctx.ProjectionCustomer, out taxRate);
					}
					else
					{
						expando._OldPrice = product.OldPrice;
					}
				}
			}

			// detach entity
			try
			{
				_services.DbContext.DetachEntity<Product>(product);
			}
			catch { }

			return expando as ExpandoObject;
		}

		private void DeployFileSystem(ExportProfileTaskContext ctx, ExportDeployment deployment)
		{
			string folderDestination = null;

			if (deployment.IsPublic)
			{
				folderDestination = Path.Combine(HttpRuntime.AppDomainAppPath, _publicFolder);
			}
			else if (deployment.FileSystemPath.IsEmpty())
			{
				return;
			}
			else if (deployment.FileSystemPath.StartsWith("/") || deployment.FileSystemPath.StartsWith("\\") || !Path.IsPathRooted(deployment.FileSystemPath))
			{
				folderDestination = CommonHelper.MapPath(deployment.FileSystemPath);
			}
			else
			{
				folderDestination = deployment.FileSystemPath;
			}

			if (!System.IO.Directory.Exists(folderDestination))
			{
				System.IO.Directory.CreateDirectory(folderDestination);
			}

			if (deployment.CreateZip)
			{
				var path = Path.Combine(folderDestination, ctx.Profile.FolderName + ".zip");
				FileSystemHelper.Copy(ctx.ZipPath, path);

				ctx.Log.Information("Copied ZIP archive " + path);
			}
			else
			{
				FileSystemHelper.CopyDirectory(new DirectoryInfo(ctx.FolderContent), new DirectoryInfo(folderDestination));

				ctx.Log.Information("Copied export data to " + folderDestination);
			}
		}

		private void DeployEmail(ExportProfileTaskContext ctx, ExportDeployment deployment)
		{
			var emailAccount = _emailAccountService.GetEmailAccountById(deployment.EmailAccountId);
			var smtpContext = new SmtpContext(emailAccount);
			var count = 0;

			foreach (var email in deployment.EmailAddresses.SplitSafe(",").Where(x => x.IsEmail()))
			{
				var queuedEmail = new QueuedEmail
				{
					From = emailAccount.Email,
					FromName = emailAccount.DisplayName,
					To = email,
					Subject = deployment.EmailSubject.NaIfEmpty(),
					CreatedOnUtc = DateTime.UtcNow,
					EmailAccountId = deployment.EmailAccountId
				};

				if (deployment.CreateZip)
				{
					queuedEmail.Attachments.Add(new QueuedEmailAttachment
					{
						StorageLocation = EmailAttachmentStorageLocation.Path,
						Path = ctx.ZipPath,
						Name = ctx.ZipName,
						MimeType = "application/zip"
					});
				}
				else
				{
					foreach (string path in System.IO.Directory.GetFiles(ctx.FolderContent, "*.*", SearchOption.AllDirectories))
					{
						string name = Path.GetFileName(path);

						queuedEmail.Attachments.Add(new QueuedEmailAttachment
						{
							StorageLocation = EmailAttachmentStorageLocation.Path,
							Path = path,
							Name = name,
							MimeType = MimeTypes.MapNameToMimeType(name)
						});
					}
				}

				_queuedEmailService.InsertQueuedEmail(queuedEmail);
				++count;
			}

			ctx.Log.Information("{0} email(s) created and queued.".FormatInvariant(count));
		}

		private void DeployHttp(ExportProfileTaskContext ctx, ExportDeployment deployment)
		{
			// TODO: make async
			var count = 0;
			string[] filePaths = System.IO.Directory.GetFiles(ctx.FolderContent, "*.*", SearchOption.AllDirectories);

			if (deployment.MultipartForm)
			{
				ICredentials credentials = null;

				if (deployment.Username.HasValue())
					credentials = new NetworkCredential(deployment.Username, deployment.Password);

				using (var handler = new HttpClientHandler { Credentials = credentials })
				using (var client = new HttpClient(handler))
				using (var formData = new MultipartFormDataContent())
				{
					foreach (var path in filePaths)
					{
						byte[] fileData = File.ReadAllBytes(path);
						formData.Add(new ByteArrayContent(fileData), "file {0}".FormatInvariant(++count), Path.GetFileName(path));
					}

					var response = client.PostAsync(deployment.Url, formData).Result;
					if (response.IsSuccessStatusCode)
					{
						ctx.Log.Information("{0} file(s) successfully uploaded as multipart form data.".FormatInvariant(count));
					}
					else if (response.Content != null)
					{
						ctx.Log.Error("Multipart form data upload failed. Response: " + response.Content.ReadAsStringAsync().Result.NaIfEmpty().Truncate(2000, "..."));
					}
				}
			}
			else
			{
			}
		}

		private void DeployFtp(ExportProfileTaskContext ctx, ExportDeployment deployment)
		{

		}

		private void ExportCoreInner(ExportProfileTaskContext ctx)
		{
			ctx.Export.StoreId = ctx.Store.Id;
			ctx.Export.StoreUrl = ctx.Store.Url;

			ctx.Export.FileNamePattern = string.Concat(
				"{0}-",
				ctx.Profile.PerStore ? SeoHelper.GetSeName(ctx.Store.Name, true, false).ToValidFileName("").Truncate(_dataExchangeSettings.MaxFileNameLength) : "all-stores",
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
					(entity, entities) => ToExpando(ctx, (Product)entity, (IEnumerable<Product>)entities),
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
					ctx.Export.SuccessfulExportedRecords = 0;

					bool goOn = ctx.Provider.Value.Execute(ctx.Export);

					ctx.Log.Information("Provider reports {0} successful exported record(s)".FormatInvariant(ctx.Export.SuccessfulExportedRecords));

					if (ctx.Cancellation.IsCancellationRequested)
						ctx.Log.Warning("Export aborted. A cancellation has been requested");

					return (goOn && !ctx.Cancellation.IsCancellationRequested);
				});
			}
		}

		private void ExportCoreOuter(ExportProfileTaskContext ctx)
		{
			if (ctx.Profile == null || !ctx.Profile.Enabled)
				return;

			var allStores = _services.StoreService.GetAllStores();
			var currentStore = _services.StoreContext.CurrentStore;

			if (ctx.Projection.CurrencyId.HasValue)
				ctx.ProjectionCurrency = _currencyService.GetCurrencyById(ctx.Projection.CurrencyId.Value);
			else
				ctx.ProjectionCurrency = _services.WorkContext.WorkingCurrency;

			if (ctx.Projection.CustomerId.HasValue)
				ctx.ProjectionCustomer = _customerService.GetCustomerById(ctx.Projection.CustomerId.Value);
			else
				ctx.ProjectionCustomer = _services.WorkContext.CurrentCustomer;

			FileSystemHelper.ClearDirectory(ctx.FolderContent, false);
			FileSystemHelper.Delete(ctx.LogPath);
			FileSystemHelper.Delete(ctx.ZipPath);

			using (var logger = new TraceLogger(ctx.LogPath))
			{
				try
				{
					ctx.Log = logger;
					ctx.Export.Log = logger;
					ctx.DeliveryTimes = _deliveryTimeService.GetAllDeliveryTimes().ToDictionary(x => x.Id, x => x);
					ctx.QuantityUnits = _quantityUnitService.GetAllQuantityUnits().ToDictionary(x => x.Id, x => x);

					var allCategories = _categoryService.GetAllCategories(showHidden: true, applyNavigationFilters: false);
					ctx.Categories = allCategories.ToDictionary(x => x.Id);

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

					if (ctx.Profile.CreateZipArchive || ctx.Profile.Deployments.Any(x => x.Enabled && x.CreateZip))
					{
						ZipFile.CreateFromDirectory(ctx.FolderContent, ctx.ZipPath, CompressionLevel.Fastest, true);
					}

					foreach (var deployment in ctx.Profile.Deployments.OrderBy(x => x.DeploymentTypeId).Where(x => x.Enabled))
					{
						try
						{
							switch (deployment.DeploymentType)
							{
								case ExportDeploymentType.FileSystem:
									DeployFileSystem(ctx, deployment);
									break;
								case ExportDeploymentType.Email:
									DeployEmail(ctx, deployment);
									break;
								case ExportDeploymentType.Http:
									DeployHttp(ctx, deployment);
									break;
								case ExportDeploymentType.Ftp:
									DeployFtp(ctx, deployment);
									break;
							}
						}
						catch (Exception exc)
						{
							logger.Error("Deployment \"{0}\" failed.".FormatInvariant(deployment.Name), exc);
						}
					}

					if (ctx.Profile.EmailAccountId != 0 && ctx.Profile.CompletedEmailAddresses.HasValue())
					{
						SendCompletionEmail(ctx);
					}
				}
				catch (Exception exc)
				{
					logger.Error(exc);
				}
				finally
				{
					try
					{
						if (ctx.Profile.Cleanup)
						{
							FileSystemHelper.ClearDirectory(ctx.FolderContent, false);
							// TODO: more cleanup if required
						}
					}
					catch { }

					try
					{
						if (ctx.Segmenter != null)
							ctx.Segmenter.Dispose();

						ctx.Export.CustomProperties.Clear();
						ctx.Export.Log = null;
						ctx.Log = null;
					}
					catch { }
				}
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
			Profile = profile;
			Provider = provider;
			Filter = XmlHelper.Deserialize<ExportFilter>(profile.Filtering);
			Projection = XmlHelper.Deserialize<ExportProjection>(profile.Projection);
			Cancellation = cancellation;

			FolderContent = FileSystemHelper.TempDir(@"Profile\Export\{0}\Content".FormatInvariant(profile.FolderName));
			FolderRoot = System.IO.Directory.GetParent(FolderContent).FullName;

			CategoryPathes = new Dictionary<int, string>();

			Export = new ExportExecuteContext(Cancellation, FolderContent);
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

		public string FolderRoot { get; private set; }
		public string FolderContent { get; private set; }
		public string ZipName
		{
			get { return Profile.FolderName + ".zip"; }
		}
		public string ZipPath
		{
			get { return Path.Combine(FolderRoot, ZipName);	}
		}
		public string LogPath
		{
			get { return Path.Combine(FolderRoot, "log.txt"); }
		}

		// data loaded once per export
		public Dictionary<int, Category> Categories { get; set; }
		public Dictionary<int, string> CategoryPathes { get; set; }
		public Dictionary<int, DeliveryTime> DeliveryTimes { get; set; }
		public Dictionary<int, QuantityUnit> QuantityUnits { get; set; }

		// data loaded once per page
		public ExportDataContext DataContext { get; set; }

		public ExportSegmenter Segmenter { get; set; }

		public ExportExecuteContext Export { get; set; }
	}
}
