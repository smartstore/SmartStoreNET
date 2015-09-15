using System;
using System.Collections.Generic;
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
using SmartStore.Collections;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Domain;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Domain.Messages;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Email;
using SmartStore.Core.Html;
using SmartStore.Core.IO;
using SmartStore.Core.Logging;
using SmartStore.Core.Plugins;
using SmartStore.Services.Catalog;
using SmartStore.Services.Common;
using SmartStore.Services.Customers;
using SmartStore.Services.Directory;
using SmartStore.Services.Helpers;
using SmartStore.Services.Localization;
using SmartStore.Services.Media;
using SmartStore.Services.Messages;
using SmartStore.Services.Orders;
using SmartStore.Services.Tasks;
using SmartStore.Services.Tax;
using SmartStore.Utilities;

namespace SmartStore.Services.DataExchange
{
	public class ExportProfileTask : ITask
	{
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
		private IProductAttributeParser _productAttributeParser;
		private IDeliveryTimeService _deliveryTimeService;
		private IQuantityUnitService _quantityUnitService;
		private IManufacturerService _manufacturerService;
		private IOrderService _orderService;
		private IAddressService _addressesService;
		private ICountryService _countryService;
		private IRepository<Order> _orderRepository;

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
			_productAttributeParser = context.Resolve<IProductAttributeParser>();
			_deliveryTimeService = context.Resolve<IDeliveryTimeService>();
			_quantityUnitService = context.Resolve<IQuantityUnitService>();
			_manufacturerService = context.Resolve<IManufacturerService>();
			_orderService = context.Resolve<IOrderService>();
			_addressesService = context.Resolve<IAddressService>();
			_countryService = context.Resolve<ICountryService>();
			_orderRepository = context.Resolve<IRepository<Order>>();
		}

		#endregion

		#region Utilities

		private ProductSearchContext GetProductSearchContext(ExportProfileTaskContext ctx, int pageIndex, int pageSize)
		{
			var searchContext = new ProductSearchContext
			{
				OrderBy = ProductSortingEnum.CreatedOn,
				PageIndex = pageIndex,
				PageSize = pageSize,
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

			if (ctx.Filter.CategoryIds != null && ctx.Filter.CategoryIds.Length > 0)
				searchContext.CategoryIds = ctx.Filter.CategoryIds.ToList();

			if (ctx.Filter.CreatedFrom.HasValue)
				searchContext.CreatedFromUtc = _dateTimeHelper.ConvertToUtcTime(ctx.Filter.CreatedFrom.Value, _dateTimeHelper.CurrentTimeZone);

			if (ctx.Filter.CreatedTo.HasValue)
				searchContext.CreatedToUtc = _dateTimeHelper.ConvertToUtcTime(ctx.Filter.CreatedTo.Value, _dateTimeHelper.CurrentTimeZone);

			return searchContext;
		}

		private void PrepareProductDescription(ExportProfileTaskContext ctx, dynamic expando, Product product)
		{
			try
			{
				var languageId = (ctx.Projection.LanguageId ?? 0);
				string description = "";

				// description merging
				if (ctx.Projection.DescriptionMerging != ExportDescriptionMerging.None)
				{
					if (ctx.Projection.DescriptionMerging == ExportDescriptionMerging.ShortDescriptionOrNameIfEmpty)
					{
						description = expando.FullDescription;

						if (description.IsEmpty())
							description = expando.ShortDescription;
						if (description.IsEmpty())
							description = expando.Name;
					}
					else if (ctx.Projection.DescriptionMerging == ExportDescriptionMerging.ShortDescription)
					{
						description = expando.ShortDescription;
					}
					else if (ctx.Projection.DescriptionMerging == ExportDescriptionMerging.Description)
					{
						description = expando.FullDescription;
					}
					else if (ctx.Projection.DescriptionMerging == ExportDescriptionMerging.NameAndShortDescription)
					{
						description = ((string)expando.Name).Grow((string)expando.ShortDescription, " ");
					}
					else if (ctx.Projection.DescriptionMerging == ExportDescriptionMerging.NameAndDescription)
					{
						description = ((string)expando.Name).Grow((string)expando.FullDescription, " ");
					}
					else if (ctx.Projection.DescriptionMerging == ExportDescriptionMerging.ManufacturerAndNameAndShortDescription ||
						ctx.Projection.DescriptionMerging == ExportDescriptionMerging.ManufacturerAndNameAndDescription)
					{
						var productManus = ctx.ProductDataContext.ProductManufacturers.Load(product.Id);

						if (productManus != null && productManus.Any())
							description = productManus.First().Manufacturer.GetLocalized(x => x.Name, languageId, true, false);

						description = description.Grow((string)expando.Name, " ");

						if (ctx.Projection.DescriptionMerging == ExportDescriptionMerging.ManufacturerAndNameAndShortDescription)
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

		private decimal? ConvertPrice(ExportProfileTaskContext ctx, Product product, decimal? price)
		{
			if (price.HasValue)
			{
				if (ctx.Projection.ConvertNetToGrossPrices)
				{
					decimal taxRate;
					price = _taxService.GetProductPrice(product, price.Value, true, ctx.ProjectionCustomer, out taxRate);
				}

				if (price != decimal.Zero)
				{
					price = _currencyService.ConvertFromPrimaryStoreCurrency(price.Value, ctx.ProjectionCurrency, ctx.Store);
				}
			}
			return price;
		}

		private decimal CalculatePrice(ExportProfileTaskContext ctx, Product product, bool forAttributeCombination)
		{
			decimal price = product.Price;
			var priceCalculationContext = ctx.ProductDataContext as PriceCalculationContext;

			// price type
			if (ctx.Projection.PriceType.HasValue && !forAttributeCombination)
			{
				if (ctx.Projection.PriceType.Value == PriceDisplayType.LowestPrice)
				{
					bool displayFromMessage;
					price = _priceCalculationService.GetLowestPrice(product, priceCalculationContext, out displayFromMessage);
				}
				else if (ctx.Projection.PriceType.Value == PriceDisplayType.PreSelectedPrice)
				{
					price = _priceCalculationService.GetPreselectedPrice(product, priceCalculationContext);
				}
				else if (ctx.Projection.PriceType.Value == PriceDisplayType.PriceWithoutDiscountsAndAttributes)
				{
					price = _priceCalculationService.GetFinalPrice(product, null, ctx.ProjectionCustomer, decimal.Zero, false, 1, null, priceCalculationContext);
				}
			}

			return ConvertPrice(ctx, product, price) ?? price;
		}

		private void GetDeliveryTimeAndQuantityUnit(ExportProfileTaskContext ctx, dynamic expando, int? deliveryTimeId, int? quantityUnitId)
		{
			if (deliveryTimeId.HasValue && ctx.DeliveryTimes.ContainsKey(deliveryTimeId.Value))
				expando.DeliveryTime = ctx.DeliveryTimes[deliveryTimeId.Value].ToExpando(ctx.Projection.LanguageId ?? 0);
			else
				expando.DeliveryTime = null;

			if (quantityUnitId.HasValue && ctx.QuantityUnits.ContainsKey(quantityUnitId.Value))
				expando.QuantityUnit = ctx.QuantityUnits[quantityUnitId.Value].ToExpando(ctx.Projection.LanguageId ?? 0);
			else
				expando.QuantityUnit = null;
		}

		private void SetProgress(ExportProfileTaskContext ctx, int loadedRecords)
		{
			if (!ctx.IsPreview && loadedRecords > 0)
			{
				int totalRecords = ctx.RecordsPerStore.Sum(x => x.Value);

				ctx.RecordCount = Math.Min(ctx.RecordCount + loadedRecords, totalRecords);

				var msg = ctx.ProgressInfo.FormatInvariant(ctx.RecordCount, totalRecords);

				ctx.TaskContext.SetProgress(ctx.RecordCount, totalRecords, msg, true);
			}
		}

		private void SendCompletionEmail(ExportProfileTaskContext ctx)
		{
			var emailAccount = _emailAccountService.GetEmailAccountById(ctx.Profile.EmailAccountId);
			var smtpContext = new SmtpContext(emailAccount);
			var message = new EmailMessage();

			var storeInfo = "{0} ({1})".FormatInvariant(ctx.Store.Name, ctx.Store.Url);

			message.To.AddRange(ctx.Profile.CompletedEmailAddresses.SplitSafe(",").Where(x => x.IsEmail()).Select(x => new EmailAddress(x)));
			message.From = new EmailAddress(emailAccount.Email, emailAccount.DisplayName);

			message.Subject = _services.Localization.GetResource("Admin.DataExchange.Export.CompletedEmail.Subject", ctx.Projection.LanguageId ?? 0)
				.FormatInvariant(ctx.Profile.Name);

			message.Body = _services.Localization.GetResource("Admin.DataExchange.Export.CompletedEmail.Body", ctx.Projection.LanguageId ?? 0)
				.FormatInvariant(storeInfo);

			_emailSender.SendEmail(smtpContext, message);
		}

		private void DeployFileSystem(ExportProfileTaskContext ctx, ExportDeployment deployment)
		{
			string folderDestination = null;

			if (deployment.IsPublic)
			{
				folderDestination = Path.Combine(HttpRuntime.AppDomainAppPath, PublicFolder);
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

				ctx.Log.Information("Copied export data files to " + folderDestination);
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

				foreach (string path in ctx.GetDeploymentFiles(deployment))
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

				_queuedEmailService.InsertQueuedEmail(queuedEmail);
				++count;
			}

			ctx.Log.Information("{0} email(s) created and queued.".FormatInvariant(count));
		}

		private async void DeployHttp(ExportProfileTaskContext ctx, ExportDeployment deployment)
		{
			var succeeded = 0;
			var url = deployment.Url;

			if (!url.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase) && !url.StartsWith("https://", StringComparison.InvariantCultureIgnoreCase))
				url = "http://" + url;

			if (deployment.HttpTransmissionType == ExportHttpTransmissionType.MultipartFormDataPost)
			{
				var count = 0;
				ICredentials credentials = null;

				if (deployment.Username.HasValue())
					credentials = new NetworkCredential(deployment.Username, deployment.Password);

				using (var handler = new HttpClientHandler { Credentials = credentials })
				using (var client = new HttpClient(handler))
				using (var formData = new MultipartFormDataContent())
				{
					foreach (var path in ctx.GetDeploymentFiles(deployment))
					{
						byte[] fileData = File.ReadAllBytes(path);
						formData.Add(new ByteArrayContent(fileData), "file {0}".FormatInvariant(++count), Path.GetFileName(path));
					}

					var response = await client.PostAsync(url, formData);

					if (response.IsSuccessStatusCode)
					{
						succeeded = count;
					}
					else if (response.Content != null)
					{
						var content = await response.Content.ReadAsStringAsync();

						var msg = "Multipart form data upload failed. {0} ({1}). Response: {2}".FormatInvariant(
							response.StatusCode.ToString(), (int)response.StatusCode, content.NaIfEmpty().Truncate(2000, "..."));

						ctx.Log.Error(msg);
					}
				}
			}
			else
			{
				using (var webClient = new WebClient())
				{
					if (deployment.Username.HasValue())
						webClient.Credentials = new NetworkCredential(deployment.Username, deployment.Password);

					foreach (var path in ctx.GetDeploymentFiles(deployment))
					{
						await webClient.UploadFileTaskAsync(url, path);
					}
				}
			}

			ctx.Log.Information("{0} file(s) successfully uploaded via HTTP.".FormatInvariant(succeeded));
		}

		private async void DeployFtp(ExportProfileTaskContext ctx, ExportDeployment deployment)
		{
			var bytesRead = 0;
			var succeededFiles = 0;
			var url = deployment.Url;
			var buffLength = 32768;
			byte[] buff = new byte[buffLength];
			var deploymentFiles = ctx.GetDeploymentFiles(deployment).ToList();
			var lastIndex = (deploymentFiles.Count - 1);

			if (!url.StartsWith("ftp://", StringComparison.InvariantCultureIgnoreCase))
				url = "ftp://" + url;

			foreach (var path in deploymentFiles)
			{
				var fileUrl = url.EnsureEndsWith("/") + Path.GetFileName(path);

				var request = (FtpWebRequest)WebRequest.Create(fileUrl);
				request.Method = WebRequestMethods.Ftp.UploadFile;
				request.KeepAlive = (deploymentFiles.IndexOf(path) != lastIndex);
				request.UseBinary = true;
				request.Proxy = null;
				request.UsePassive = deployment.PassiveMode;
				request.EnableSsl = deployment.UseSsl;

				if (deployment.Username.HasValue())
					request.Credentials = new NetworkCredential(deployment.Username, deployment.Password);

				request.ContentLength = (new FileInfo(path)).Length;

				var requestStream = await request.GetRequestStreamAsync();

				using (var stream = new FileStream(path, FileMode.Open))
				{
					while ((bytesRead = stream.Read(buff, 0, buffLength)) != 0)
					{
						await requestStream.WriteAsync(buff, 0, bytesRead);
					}
				}

				requestStream.Close();

				var response = (FtpWebResponse)await request.GetResponseAsync();
				var statusCode = (int)response.StatusCode;

				if (statusCode >= 200 && statusCode <= 299)
				{
					++succeededFiles;
				}
				else
				{
					var msg = "The FTP transfer might fail. {0} ({1}), {2}. File {3}".FormatInvariant(
						response.StatusCode.ToString(), statusCode, response.StatusDescription.NaIfEmpty(), path);

					ctx.Log.Error(msg);
				}
			}

			ctx.Log.Information("{0} file(s) successfully uploaded via FTP.".FormatInvariant(succeededFiles));
		}

		#endregion

		#region Segmenter callbacks

		private List<Product> GetProducts(ExportProfileTaskContext ctx, int pageIndex)
		{
			var result = new List<Product>();

			var searchContext = GetProductSearchContext(ctx, pageIndex, ctx.PageSize);

			var products = _productService.SearchProducts(searchContext);

			foreach (var product in products)
			{
				if (product.ProductType == ProductType.SimpleProduct || product.ProductType == ProductType.BundledProduct)
				{
					result.Add(product);
				}
				else if (product.ProductType == ProductType.GroupedProduct)
				{
					if (ctx.IsPreview)
					{
						result.Add(product);
					}
					else
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
				}
			}

			// load data behind navigation properties for current page in one go
			ctx.ProductDataContext = new ExportProductDataContext(products,
				x => _productAttributeService.GetProductVariantAttributesByProductIds(x, null),
				x => _productAttributeService.GetProductVariantAttributeCombinations(x),
				x => _productService.GetTierPrices(x, ctx.ProjectionCustomer, ctx.Store.Id),
				x => _categoryService.GetProductCategoriesByProductIds(x),
				x => _manufacturerService.GetProductManufacturersByProductIds(x),
				x => _productService.GetProductPicturesByProductIds(x)
			);

			SetProgress(ctx, products.Count);

			try
			{
				_services.DbContext.DetachEntities<Product>(result);
			}
			catch { }

			return result;
		}

		private List<Order> GetOrders(ExportProfileTaskContext ctx, int pageIndex, int pageSize, out int totalCount)
		{
			var orders = _orderService.SearchOrders(
				ctx.Profile.PerStore ? ctx.Store.Id : ctx.Filter.StoreId,
				ctx.Projection.CustomerId ?? 0,
				ctx.Filter.CreatedFrom.HasValue ? (DateTime?)_dateTimeHelper.ConvertToUtcTime(ctx.Filter.CreatedFrom.Value, _dateTimeHelper.CurrentTimeZone) : null,
				ctx.Filter.CreatedTo.HasValue ? (DateTime?)_dateTimeHelper.ConvertToUtcTime(ctx.Filter.CreatedTo.Value, _dateTimeHelper.CurrentTimeZone) : null,
				ctx.Filter.OrderStatusIds,
				ctx.Filter.PaymentStatusIds,
				ctx.Filter.ShippingStatusIds,
				null,
				null,
				null,
				pageIndex,
				pageSize,
				null,
				ctx.EntityIdsSelected
			);

			totalCount = orders.TotalCount;

			if (ctx.Projection.OrderStatusChange != ExportOrderStatusChange.None)
			{
				ctx.EntityIdsLoaded = ctx.EntityIdsLoaded
					.Union(orders.Select(x => x.Id))
					.Distinct()
					.ToList();
			}

			var result = orders as List<Order>;

			if (pageSize > 1)
			{
				ctx.OrderDataContext = new ExportOrderDataContext(result,
					x => _customerService.GetCustomersByIds(x),
					x => _addressesService.GetAddressByIds(x),
					x => _orderService.GetOrderItemsByOrderIds(x)
				);

				SetProgress(ctx, orders.Count);
			}

			try
			{
				_services.DbContext.DetachEntities<Order>(result);

				// TODO: examine remaining attached entities
				//foreach (var item in orderItems)
				//	_services.DbContext.DetachEntity<Product>(item.Product);
			}
			catch { }

			return result;
		}

		private List<ExpandoObject> ToExpando(ExportProfileTaskContext ctx, Product product)
		{
			var result = new List<ExpandoObject>();
			var languageId = (ctx.Projection.LanguageId ?? 0);
			var productPictures = ctx.ProductDataContext.ProductPictures.Load(product.Id);
			var productManufacturers = ctx.ProductDataContext.ProductManufacturers.Load(product.Id);
			var productCategories = ctx.ProductDataContext.ProductCategories.Load(product.Id);
			var productAttributes = ctx.ProductDataContext.Attributes.Load(product.Id);
			var productAttributeCombinations = ctx.ProductDataContext.AttributeCombinations.Load(product.Id);

			dynamic expando = product.ToExpando(languageId);

			expando._DetailUrl = ctx.Store.Url + expando.SeName;

			expando._CategoryName = null;

			expando._CategoryPath = _categoryService.GetCategoryPath(
				product,
				null,
				x => ctx.CategoryPathes.ContainsKey(x) ? ctx.CategoryPathes[x] : null,
				(id, value) => ctx.CategoryPathes[id] = value,
				x => ctx.Categories.ContainsKey(x) ? ctx.Categories[x] : _categoryService.GetCategoryById(x),
				productCategories.OrderBy(x => x.DisplayOrder).FirstOrDefault()
			);

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

					if (expando._CategoryName == null)
						expando._CategoryName = (string)exp.Category.Name;

					return exp as ExpandoObject;
				})
				.ToList();

			expando.ProductAttributes = productAttributes
				.OrderBy(x => x.DisplayOrder)
				.Select(x => x.ToExpando(languageId))
				.ToList();

			expando.ProductAttributeCombinations = productAttributeCombinations
				.Select(x =>
				{
					dynamic exp = x.ToExpando();

					GetDeliveryTimeAndQuantityUnit(ctx, expando, x.DeliveryTimeId, x.QuantityUnitId);

					return exp as ExpandoObject;
				})
				.ToList();


			// data controlled through ExportProjectionSupport attribute
			if (ctx.Provider.Supports(ExportProjectionSupport.Description))
			{
				PrepareProductDescription(ctx, expando, product);
			}

			if (ctx.Provider.Supports(ExportProjectionSupport.Brand))
			{
				string brand = null;
				var productManus = ctx.ProductDataContext.ProductManufacturers.Load(product.Id);

				if (productManus != null && productManus.Any())
					brand = productManus.First().Manufacturer.GetLocalized(x => x.Name, languageId, true, false);

				if (brand.IsEmpty())
					brand = ctx.Projection.Brand;

				expando._Brand = brand;
			}

			if (ctx.Provider.Supports(ExportProjectionSupport.MainPictureUrl))
			{
				if (productPictures != null && productPictures.Any())
					expando._MainPictureUrl = _pictureService.GetPictureUrl(productPictures.First().Picture, ctx.Projection.PictureSize, storeLocation: ctx.Store.Url);
				else
					expando._MainPictureUrl = _pictureService.GetDefaultPictureUrl(ctx.Projection.PictureSize, storeLocation: ctx.Store.Url);
			}


			Action<dynamic, ProductVariantAttributeCombination> matterOfDataMerging = (exp, combination) =>
			{
				product.MergeWithCombination(combination);

				exp.Price = CalculatePrice(ctx, product, combination != null);
				exp.StockQuantity = product.StockQuantity;
				exp.BackorderModeId = product.BackorderModeId;
				exp.Sku = product.Sku;
				exp.Gtin = product.Gtin;
				exp.ManufacturerPartNumber = product.ManufacturerPartNumber;
				exp.DeliveryTimeId = product.DeliveryTimeId;
				exp.QuantityUnitId = product.QuantityUnitId;
				exp.Length = product.Length;
				exp.Width = product.Width;
				exp.Height = product.Height;
				exp.BasePriceAmount = product.BasePriceAmount;
				exp.BasePriceBaseAmount = product.BasePriceBaseAmount;

				if (combination != null && ctx.Projection.AttributeCombinationValueMerging == ExportAttributeValueMerging.AppendAllValuesToName)
				{
					var values = _productAttributeParser.ParseProductVariantAttributeValues(combination.AttributesXml, productAttributes, languageId);
					exp.Name = ((string)exp.Name).Grow(string.Join(", ", values), " ");
				}

				exp._BasePriceInfo = product.GetBasePriceInfo(_services.Localization, _priceFormatter, decimal.Zero, true);

				// navigation properties
				GetDeliveryTimeAndQuantityUnit(ctx, exp, product.DeliveryTimeId, product.QuantityUnitId);

				if (ctx.Provider.Supports(ExportProjectionSupport.UseOwnProductNo) && product.ManufacturerPartNumber.IsEmpty())
				{
					exp.ManufacturerPartNumber = product.Sku;
				}

				if (ctx.Provider.Supports(ExportProjectionSupport.ShippingTime))
				{
					dynamic deliveryTime = exp.DeliveryTime;
					exp._ShippingTime = (deliveryTime == null ? ctx.Projection.ShippingTime : deliveryTime.Name);
				}

				if (ctx.Provider.Supports(ExportProjectionSupport.ShippingCosts))
				{
					exp._FreeShippingThreshold = ctx.Projection.FreeShippingThreshold;

					if (product.IsFreeShipping || (ctx.Projection.FreeShippingThreshold.HasValue && (decimal)exp.Price >= ctx.Projection.FreeShippingThreshold.Value))
						exp._ShippingCosts = decimal.Zero;
					else
						exp._ShippingCosts = ctx.Projection.ShippingCosts;
				}

				if (ctx.Provider.Supports(ExportProjectionSupport.OldPrice))
				{
					if (product.OldPrice != decimal.Zero && product.OldPrice != (decimal)exp.Price && !(product.ProductType == ProductType.BundledProduct && product.BundlePerItemPricing))
					{
						if (ctx.Projection.ConvertNetToGrossPrices)
						{
							decimal taxRate;
							exp._OldPrice = _taxService.GetProductPrice(product, product.OldPrice, true, ctx.ProjectionCustomer, out taxRate);
						}
						else
						{
							exp._OldPrice = product.OldPrice;
						}
					}
					else
					{
						exp._OldPrice = null;
					}
				}

				if (ctx.Provider.Supports(ExportProjectionSupport.SpecialPrice))
				{
					exp._SpecialPrice = ConvertPrice(ctx, product, _priceCalculationService.GetSpecialPrice(product));
				}
			};


			// yield return expando
			if (ctx.Projection.AttributeCombinationAsProduct && productAttributeCombinations.Where(x => x.IsActive).Count() > 0)
			{
				// EF does not support entities to be cconstructed in a LINQ to entities query.
				// So it's not possible to join-query attribute combinations and products without losing ProductSearchContext ability.
				// We are reduced to somewhat compound data here.

				foreach (var combination in productAttributeCombinations.Where(x => x.IsActive))
				{
					var expandoCombination = ((IDictionary<string, object>)expando).ToExpandoObject();	// clone

					matterOfDataMerging(expandoCombination, combination);
					result.Add(expandoCombination);
				}
			}
			else
			{
				matterOfDataMerging(expando, null);
				result.Add(expando);
			}

			return result;
		}

		private List<ExpandoObject> ToExpando(ExportProfileTaskContext ctx, Order order)
		{
			var result = new List<ExpandoObject>();
			var languageId = (ctx.Projection.LanguageId ?? 0);

			ctx.OrderDataContext.Addresses.Collect(order.ShippingAddressId ?? 0);

			var customers = ctx.OrderDataContext.Customers.Load(order.CustomerId);
			var addresses = ctx.OrderDataContext.Addresses.Load(order.BillingAddressId);
			var orderItems = ctx.OrderDataContext.OrderItems.Load(order.Id);

			dynamic expando = order.ToExpando(languageId, _services.Localization);

			expando.Customer = customers
				.FirstOrDefault(x => x.Id == order.CustomerId)
				.ToExpando();

			expando.BillingAddress = addresses
				.FirstOrDefault(x => x.Id == order.BillingAddressId)
				.ToExpando(languageId);

			if (order.ShippingAddressId.HasValue)
				expando.ShippingAddress = addresses.FirstOrDefault(x => x.Id == order.ShippingAddressId.Value).ToExpando(languageId);
			else
				expando.ShippingAddress = null;

			if (ctx.Stores.ContainsKey(order.StoreId))
				expando.Store = ctx.Stores[order.StoreId].ToExpando(languageId);
			else
				expando.Store = null;

			expando.OrderItems = orderItems
				.Select(e =>
				{
					dynamic exp = e.ToExpando(languageId);

					exp._BasePriceInfo = e.Product.GetBasePriceInfo(_services.Localization, _priceFormatter, decimal.Zero, true);

					GetDeliveryTimeAndQuantityUnit(ctx, exp, e.Product.DeliveryTimeId, e.Product.QuantityUnitId);

					return exp as ExpandoObject;
				})
				.ToList();

			return result;
		}

		#endregion

		private List<Store> Init(ExportProfileTaskContext ctx)
		{
			List<Store> result = null;

			if (ctx.Projection.CurrencyId.HasValue)
				ctx.ProjectionCurrency = _currencyService.GetCurrencyById(ctx.Projection.CurrencyId.Value);
			else
				ctx.ProjectionCurrency = _services.WorkContext.WorkingCurrency;

			if (ctx.Projection.CustomerId.HasValue)
				ctx.ProjectionCustomer = _customerService.GetCustomerById(ctx.Projection.CustomerId.Value);
			else
				ctx.ProjectionCustomer = _services.WorkContext.CurrentCustomer;

			ctx.Stores = _services.StoreService.GetAllStores().ToDictionary(x => x.Id, x => x);

			if (!ctx.IsPreview && ctx.Profile.PerStore)
			{
				result = new List<Store>(ctx.Stores.Values.Where(x => x.Id == ctx.Filter.StoreId || ctx.Filter.StoreId == 0));
			}
			else
			{
				int? storeId = (ctx.Filter.StoreId == 0 ? ctx.Projection.StoreId : ctx.Filter.StoreId);

				ctx.Store = ctx.Stores.Values.FirstOrDefault(x => x.Id == (storeId ?? _services.StoreContext.CurrentStore.Id));

				result = new List<Store> { ctx.Store };
			}

			// get total records for progress
			foreach (var store in result)
			{
				if (ctx.TotalRecords.HasValue)
				{
					ctx.RecordsPerStore.Add(store.Id, ctx.TotalRecords.Value);
				}
				else if (ctx.Provider.Value.EntityType == ExportEntityType.Product)
				{
					ctx.Store = store;
					var searchContext = GetProductSearchContext(ctx, ctx.Profile.Offset, 1);
					var anySingleProduct = _productService.SearchProducts(searchContext);
					ctx.RecordsPerStore.Add(store.Id, anySingleProduct.TotalCount);
				}
				else if (ctx.Provider.Value.EntityType == ExportEntityType.Order)
				{
					ctx.Store = store;
					int totalCount = 0;
					var unused = GetOrders(ctx, 0, 1, out totalCount);
					ctx.RecordsPerStore.Add(store.Id, totalCount);
				}
			}

			return result;
		}

		private void ExportCoreInner(ExportProfileTaskContext ctx, Store store)
		{
			if (ctx.Export.Abort != ExportAbortion.None)
				return;

			ctx.Store = store;

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

			ctx.Export.Store = ctx.Store.ToExpando(ctx.Projection.LanguageId ?? 0);

			ctx.Export.MaxFileNameLength = _dataExchangeSettings.MaxFileNameLength;

			ctx.Export.FileExtension = ctx.Provider.Value.FileExtension.ToLower().EnsureStartsWith(".");

			ctx.Export.FileNamePattern = ctx.Profile.FileNamePattern
				.Replace("%ExportProfile.Id%", ctx.Profile.Id.ToString())
				.Replace("%ExportProfile.SeoName%", SeoHelper.GetSeName(ctx.Profile.Name, true, false).Replace("/", "").Replace("-", ""))
				.Replace("%ExportProfile.FolderName%", ctx.Profile.FolderName)
				.Replace("%Store.Id%", ctx.Store.Id.ToString())
				.Replace("%Store.SeoName%", ctx.Profile.PerStore ? SeoHelper.GetSeName(ctx.Store.Name, true, false) : "allstores");


			var totalCount = ctx.RecordsPerStore.First(x => x.Key == ctx.Store.Id).Value;

			if (ctx.Provider.Value.EntityType == ExportEntityType.Product)
			{
				ctx.Export.Data = new ExportSegmenter<Product>(
					pageIndex => GetProducts(ctx, pageIndex),
					entity => ToExpando(ctx, entity),
					new PagedList(ctx.Profile.Offset, ctx.Profile.Limit, ctx.PageIndex, ctx.PageSize, totalCount),
					ctx.IsPreview ? 0 : ctx.Profile.BatchSize
				);
			}
			else if (ctx.Provider.Value.EntityType == ExportEntityType.Order)
			{
				int unused;

				ctx.Export.Data = new ExportSegmenter<Order>(
					pageIndex => GetOrders(ctx, pageIndex, ctx.PageSize, out unused),
					entity => ToExpando(ctx, entity),
					new PagedList(ctx.Profile.Offset, ctx.Profile.Limit, ctx.PageIndex, ctx.PageSize, totalCount),
					ctx.IsPreview ? 0 : ctx.Profile.BatchSize
				);
			}

			if (ctx.Export.Data == null)
			{
				throw new SmartException("Unsupported entity type '{0}'".FormatInvariant(ctx.Provider.Value.EntityType.ToString()));
			}
			else
			{
				(ctx.Export.Data as IExportExecuter).Start(() =>
				{
					ctx.Export.RecordsSucceeded = 0;

					if (!ctx.IsPreview)
					{
						var finallyResolvedPattern = ctx.Export.FileNamePattern
							.Replace("%Misc.FileNumber%", (ctx.Export.Data.FileIndex + 1).ToString("D4"))
							.ToValidFileName("")
							.Truncate(ctx.Export.MaxFileNameLength);

						ctx.Export.FileName = finallyResolvedPattern + ctx.Export.FileExtension;

						ctx.Provider.Value.Execute(ctx.Export);

						ctx.Log.Information("Provider reports {0} successful exported record(s)".FormatInvariant(ctx.Export.RecordsSucceeded));

						if (File.Exists(ctx.Export.FilePath))
						{
							ctx.ResultInfo.Files.Add(new ExportResultFileInfo
							{
								StoreId = ctx.Store.Id,
								FileName = ctx.Export.FileName
							});
						}
					}
					else if (ctx.Export.Data.ReadNextSegment())
					{
						var segment = ctx.Export.Data.CurrentSegment;

						foreach (dynamic record in segment)
						{
							ctx.PreviewData(record);
						}
					}

					if (ctx.Export.IsMaxFailures)
						ctx.Log.Warning("Export aborted. The maximum number of failures has been reached");

					if (ctx.TaskContext.CancellationToken.IsCancellationRequested)
						ctx.Log.Warning("Export aborted. A cancellation has been requested");

					return (ctx.Export.Abort == ExportAbortion.None);
				});

				if (ctx.Export.Abort != ExportAbortion.Hard)
				{
					ctx.Provider.Value.ExecuteEnded(ctx.Export);
				}
			}
		}

		private void ExportCoreOuter(ExportProfileTaskContext ctx)
		{
			if (ctx.Profile == null || !ctx.Profile.Enabled)
				return;

			FileSystemHelper.Delete(ctx.LogPath);

			if (!ctx.IsPreview)
			{
				FileSystemHelper.ClearDirectory(ctx.FolderContent, false);
				FileSystemHelper.Delete(ctx.ZipPath);
			}

			using (var logger = new TraceLogger(ctx.LogPath))
			{
				try
				{
					if (ctx.Provider == null)
					{
						throw new SmartException("Export aborted because the export provider cannot be loaded");
					}

					if (!ctx.Provider.IsValid())
					{
						throw new SmartException("Export aborted because the export provider is not valid");
					}

					ctx.Log = logger;
					ctx.Export.Log = logger;
					ctx.ProgressInfo = _services.Localization.GetResource("Admin.DataExchange.Export.ProgressInfo");

					if (ctx.Profile.ProviderConfigData.HasValue())
					{
						string partialName;
						Type dataType;
						Action<object> initialize;
						if (ctx.Provider.Value.RequiresConfiguration(out partialName, out dataType, out initialize))
						{
							ctx.Export.ConfigurationData = XmlHelper.Deserialize(ctx.Profile.ProviderConfigData, dataType);
						}
					}

					using (var scope = new DbContextScope(_services.DbContext, autoDetectChanges: false, proxyCreation: true, validateOnSave: false, forceNoTracking: true))
					{
						ctx.DeliveryTimes = _deliveryTimeService.GetAllDeliveryTimes().ToDictionary(x => x.Id, x => x);
						ctx.QuantityUnits = _quantityUnitService.GetAllQuantityUnits().ToDictionary(x => x.Id, x => x);

						if (ctx.Provider.Value.EntityType == ExportEntityType.Product)
						{
							var allCategories = _categoryService.GetAllCategories(showHidden: true, applyNavigationFilters: false);
							ctx.Categories = allCategories.ToDictionary(x => x.Id);
						}

						if (ctx.Provider.Value.EntityType == ExportEntityType.Order)
						{
							ctx.Countries = _countryService.GetAllCountries(true).ToDictionary(x => x.Id, x => x);
						}

						var stores = Init(ctx);

						ctx.Export.Customer = ctx.ProjectionCustomer.ToExpando();
						ctx.Export.Currency = ctx.ProjectionCurrency.ToExpando(ctx.Projection.LanguageId ?? 0);

						stores.ForEach(x => ExportCoreInner(ctx, x));
					}

					if (!(ctx.IsPreview || ctx.Export.Abort == ExportAbortion.Hard))
					{
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
								logger.Error("Deployment \"{0}\" of type {1} failed.".FormatInvariant(deployment.Name, deployment.DeploymentType.ToString()), exc);
							}
						}

						if (ctx.Profile.EmailAccountId != 0 && ctx.Profile.CompletedEmailAddresses.HasValue())
						{
							SendCompletionEmail(ctx);
						}
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
						if (!ctx.IsPreview)
						{
							ctx.Profile.ResultInfo = XmlHelper.Serialize<ExportResultInfo>(ctx.ResultInfo);

							_exportService.UpdateExportProfile(ctx.Profile);
						}
					}
					catch { }

					try
					{
						if (!ctx.IsPreview && ctx.Profile.Cleanup && ctx.Export.Abort != ExportAbortion.Hard)
						{
							FileSystemHelper.ClearDirectory(ctx.FolderContent, false);
						}
					}
					catch { }

					try
					{
						ctx.Countries.Clear();
						ctx.Stores.Clear();
						ctx.QuantityUnits.Clear();
						ctx.DeliveryTimes.Clear();
						ctx.CategoryPathes.Clear();
						ctx.Categories.Clear();
						ctx.EntityIdsSelected.Clear();
						ctx.ProductDataContext = null;
						ctx.OrderDataContext = null;

						(ctx.Export.Data as IExportExecuter).Dispose();

						ctx.Export.CustomProperties.Clear();
						ctx.Export.Log = null;
						ctx.Log = null;
					}
					catch { }
				}
			}

			if (ctx.IsPreview || ctx.Export.Abort == ExportAbortion.Hard)
				return;

			// post process order entities
			if (ctx.EntityIdsLoaded.Count > 0 && ctx.Provider.Value.EntityType == ExportEntityType.Order && ctx.Projection.OrderStatusChange != ExportOrderStatusChange.None)
			{
				using (var logger = new TraceLogger(ctx.LogPath))
				{
					try
					{
						int? orderStatusId = null;

						if (ctx.Projection.OrderStatusChange == ExportOrderStatusChange.Processing)
							orderStatusId = (int)OrderStatus.Processing;
						else if (ctx.Projection.OrderStatusChange == ExportOrderStatusChange.Complete)
							orderStatusId = (int)OrderStatus.Complete;

						using (var scope = new DbContextScope(_services.DbContext, false, null, false, false, false, false))
						{
							foreach (var chunk in ctx.EntityIdsLoaded.Chunk())
							{
								var entities = _orderRepository.Table.Where(x => chunk.Contains(x.Id)).ToList();

								entities.ForEach(x => x.OrderStatusId = (orderStatusId ?? x.OrderStatusId));

								_services.DbContext.SaveChanges();
							}
						}

						logger.Information("Updated order status for {0} order(s).".FormatInvariant(ctx.EntityIdsLoaded.Count()));
					}
					catch (Exception exc)
					{
						logger.Error(exc);
					}
				}
			}
		}

		public static string PublicFolder
		{
			get { return "Exchange"; }
		}

		public void Execute(TaskExecutionContext context)
		{
			InitDependencies(context);

			var id = context.ScheduleTask.Alias.ToInt();
			var profile = _exportService.GetExportProfileById(id);

			var selectedIdsCacheKey = "ExportTaskSelectedIds" + id.ToString();
			var selectedIds = HttpRuntime.Cache[selectedIdsCacheKey] as string;

			var ctx = new ExportProfileTaskContext(context, profile, _exportService.LoadProvider(profile.ProviderSystemName), selectedIds);

			HttpRuntime.Cache.Remove(selectedIdsCacheKey);

			ExportCoreOuter(ctx);

			context.CancellationToken.ThrowIfCancellationRequested();
		}

		public void Preview(ExportProfile profile, IComponentContext context, int pageIndex, int pageSize, int totalRecords, Action<dynamic> previewData)
		{
			if (profile == null)
				throw new ArgumentNullException("profile");

			if (context == null)
				throw new ArgumentNullException("context");

			var taskContext = new TaskExecutionContext(context, null);
			var cancellation = new CancellationTokenSource(TimeSpan.FromMinutes(5.0));

			taskContext.CancellationToken = cancellation.Token;

			InitDependencies(taskContext);

			var ctx = new ExportProfileTaskContext(taskContext, profile, _exportService.LoadProvider(profile.ProviderSystemName), null, 
				pageIndex, pageSize, totalRecords, previewData);

			ExportCoreOuter(ctx);
		}

		public int GetRecordCount(ExportProfile profile, Provider<IExportProvider> provider, IComponentContext context)
		{
			if (profile == null)
				throw new ArgumentNullException("profile");

			if (provider == null)
				throw new ArgumentNullException("provider");

			if (context == null)
				throw new ArgumentNullException("context");

			var taskContext = new TaskExecutionContext(context, null);
			var cancellation = new CancellationTokenSource(TimeSpan.FromMinutes(5.0));

			taskContext.CancellationToken = cancellation.Token;

			InitDependencies(taskContext);

			var ctx = new ExportProfileTaskContext(taskContext, profile, provider, previewData: x => { });

			var unused = Init(ctx);

			int result = ctx.RecordsPerStore.First().Value;

			return result;
		}
	}


	internal class ExportProfileTaskContext
	{
		private ExportProductDataContext _productDataContext;
		private ExportOrderDataContext _orderDataContext;

		public ExportProfileTaskContext(
			TaskExecutionContext taskContext,
			ExportProfile profile, 
			Provider<IExportProvider> provider,
			string selectedIds = null,
			int pageIndex = 0,
			int pageSize = 100,
			int? totalRecords = null,
			Action<dynamic> previewData = null)
		{
			TaskContext = taskContext;
			Profile = profile;
			Provider = provider;
			Filter = XmlHelper.Deserialize<ExportFilter>(profile.Filtering);
			Projection = XmlHelper.Deserialize<ExportProjection>(profile.Projection);
			EntityIdsSelected = selectedIds.SplitSafe(",").Select(x => x.ToInt()).ToList();
			PageIndex = pageIndex;
			PageSize = pageSize;
			TotalRecords = totalRecords;
			PreviewData = previewData;

			FolderContent = FileSystemHelper.TempDir(@"Profile\Export\{0}\Content".FormatInvariant(profile.FolderName));
			FolderRoot = System.IO.Directory.GetParent(FolderContent).FullName;

			Categories = new Dictionary<int, Category>();
			CategoryPathes = new Dictionary<int, string>();
			Countries = new Dictionary<int, Country>();

			Export = new ExportExecuteContext(TaskContext.CancellationToken, FolderContent);
			Export.Projection = XmlHelper.Deserialize<ExportProjection>(profile.Projection);

			RecordsPerStore = new Dictionary<int, int>();
			EntityIdsLoaded = new List<int>();

			ResultInfo = new ExportResultInfo
			{
				Files = new List<ExportResultFileInfo>()
			};
		}

		public List<int> EntityIdsSelected { get; private set; }
		public List<int> EntityIdsLoaded { get; set; }

		public int PageIndex { get; private set; }
		public int PageSize { get; private set; }
		public int? TotalRecords { get; set; }
		public int RecordCount { get; set; }
		public Dictionary<int, int> RecordsPerStore { get; set; }
		public string ProgressInfo { get; set; }

		public Action<dynamic> PreviewData { get; private set; }
		public bool IsPreview
		{
			get { return PreviewData != null; }
		}

		public TaskExecutionContext TaskContext { get; private set; }
		public ExportProfile Profile { get; private set; }
		public Provider<IExportProvider> Provider { get; private set; }
		public ExportResultInfo ResultInfo { get; set; }

		public ExportFilter Filter { get; private set; }
		public ExportProjection Projection { get; private set; }
		public Currency ProjectionCurrency { get; set; }
		public Customer ProjectionCustomer { get; set; }

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

		public string[] GetDeploymentFiles(ExportDeployment deployment)
		{
			if (deployment.CreateZip)
				return new string[] { ZipPath };

			return System.IO.Directory.GetFiles(FolderContent, "*.*", SearchOption.AllDirectories);
		}

		// data loaded once per export
		public Dictionary<int, Category> Categories { get; set; }
		public Dictionary<int, string> CategoryPathes { get; set; }
		public Dictionary<int, DeliveryTime> DeliveryTimes { get; set; }
		public Dictionary<int, QuantityUnit> QuantityUnits { get; set; }
		public Dictionary<int, Store> Stores { get; set; }
		public Dictionary<int, Country> Countries { get; set; }

		// data loaded once per page
		public ExportProductDataContext ProductDataContext
		{
			get
			{
				return _productDataContext;
			}
			set
			{
				if (_productDataContext != null)
					_productDataContext.Clear();

				_productDataContext = value;
			}
		}
		public ExportOrderDataContext OrderDataContext
		{
			get
			{
				return _orderDataContext;
			}
			set
			{
				if (_orderDataContext != null)
					_orderDataContext.Clear();

				_orderDataContext = value;
			}
		}

		public ExportExecuteContext Export { get; set; }
	}
}
