using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Autofac;
using SmartStore.Core;
using SmartStore.Core.Async;
using SmartStore.Core.Data;
using SmartStore.Core.Domain;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Discounts;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Domain.Messages;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Seo;
using SmartStore.Core.Domain.Shipping;
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
using SmartStore.Services.Seo;
using SmartStore.Services.Shipping;
using SmartStore.Services.Tasks;
using SmartStore.Services.Tax;
using SmartStore.Utilities;
using SmartStore.Utilities.Threading;

// note: namespace persisted in ScheduleTask.Type
namespace SmartStore.Services.DataExchange.ExportTask
{
	public class ExportProfileTask : ITask
	{
		private static readonly ReaderWriterLockSlim _rwLock = new ReaderWriterLockSlim();

		#region Dependencies

		private ICommonServices _services;
		private Lazy<IExportService> _exportService;
		private Lazy<DataExchangeSettings> _dataExchangeSettings;
		private Lazy<IProductService> _productService;
		private Lazy<IPictureService> _pictureService;
		private Lazy<MediaSettings> _mediaSettings;
		private Lazy<IPriceCalculationService> _priceCalculationService;
		private Lazy<ITaxService> _taxService;
		private Lazy<ICurrencyService> _currencyService;
		private Lazy<ICustomerService> _customerService;
		private Lazy<IRepository<Customer>> _customerRepository;
		private Lazy<ICategoryService> _categoryService;
		private Lazy<IPriceFormatter> _priceFormatter;
		private Lazy<IDateTimeHelper> _dateTimeHelper;
		private Lazy<IEmailAccountService> _emailAccountService;
		private Lazy<IEmailSender> _emailSender;
		private Lazy<IQueuedEmailService> _queuedEmailService;
		private Lazy<IProductAttributeService> _productAttributeService;
		private Lazy<IProductAttributeParser> _productAttributeParser;
		private Lazy<IDeliveryTimeService> _deliveryTimeService;
		private Lazy<IQuantityUnitService> _quantityUnitService;
		private Lazy<IManufacturerService> _manufacturerService;
		private Lazy<IOrderService> _orderService;
		private Lazy<IAddressService> _addressesService;
		private Lazy<ICountryService> _countryService;
		private Lazy<IRepository<Order>> _orderRepository;
		private Lazy<ILanguageService> _languageService;
		private Lazy<IShipmentService> _shipmentService;
		private Lazy<IProductTemplateService> _productTemplateService;
		private Lazy<ILocalizedEntityService> _localizedEntityService;
		private Lazy<IUrlRecordService> _urlRecordService;
		private Lazy<IGenericAttributeService> _genericAttributeService;
		private Lazy<IRepository<NewsLetterSubscription>> _subscriptionRepository;

		private void InitDependencies(TaskExecutionContext context)
		{
			_services = context.Resolve<ICommonServices>();
			_exportService = context.Resolve<Lazy<IExportService>>();
			_dataExchangeSettings = context.Resolve<Lazy<DataExchangeSettings>>();
			_productService = context.Resolve<Lazy<IProductService>>();
			_pictureService = context.Resolve<Lazy<IPictureService>>();
			_mediaSettings = context.Resolve<Lazy<MediaSettings>>();
			_priceCalculationService = context.Resolve<Lazy<IPriceCalculationService>>();
			_taxService = context.Resolve<Lazy<ITaxService>>();
			_currencyService = context.Resolve<Lazy<ICurrencyService>>();
			_customerService = context.Resolve<Lazy<ICustomerService>>();
			_customerRepository = context.Resolve<Lazy<IRepository<Customer>>>();
			_categoryService = context.Resolve<Lazy<ICategoryService>>();
			_priceFormatter = context.Resolve<Lazy<IPriceFormatter>>();
			_dateTimeHelper = context.Resolve<Lazy<IDateTimeHelper>>();
			_emailAccountService = context.Resolve<Lazy<IEmailAccountService>>();
			_emailSender = context.Resolve<Lazy<IEmailSender>>();
			_queuedEmailService = context.Resolve<Lazy<IQueuedEmailService>>();
			_productAttributeService = context.Resolve<Lazy<IProductAttributeService>>();
			_productAttributeParser = context.Resolve<Lazy<IProductAttributeParser>>();
			_deliveryTimeService = context.Resolve<Lazy<IDeliveryTimeService>>();
			_quantityUnitService = context.Resolve<Lazy<IQuantityUnitService>>();
			_manufacturerService = context.Resolve<Lazy<IManufacturerService>>();
			_orderService = context.Resolve<Lazy<IOrderService>>();
			_addressesService = context.Resolve<Lazy<IAddressService>>();
			_countryService = context.Resolve<Lazy<ICountryService>>();
			_orderRepository = context.Resolve<Lazy<IRepository<Order>>>();
			_languageService = context.Resolve<Lazy<ILanguageService>>();
			_shipmentService = context.Resolve<Lazy<IShipmentService>>();
			_productTemplateService = context.Resolve<Lazy<IProductTemplateService>>();
			_localizedEntityService = context.Resolve<Lazy<ILocalizedEntityService>>();
			_urlRecordService = context.Resolve<Lazy<IUrlRecordService>>();
			_genericAttributeService = context.Resolve<Lazy<IGenericAttributeService>>();
			_subscriptionRepository = context.Resolve<Lazy<IRepository<NewsLetterSubscription>>>();
		}

		#endregion

		#region Utilities

		private List<dynamic> GetLocalized<T>(ExportProfileTaskContext ctx, T entity, params Expression<Func<T, string>>[] keySelectors)
			where T : BaseEntity, ILocalizedEntity
		{
			if (ctx.Languages.Count <= 1)
				return null;

			var localized = new List<dynamic>();

			var localeKeyGroup = typeof(T).Name;
			var isSlugSupported = typeof(ISlugSupported).IsAssignableFrom(typeof(T));

			foreach (var language in ctx.Languages)
			{
				var languageCulture = language.Value.LanguageCulture.EmptyNull().ToLower();

				// add SeName
				if (isSlugSupported)
				{
					var value = _urlRecordService.Value.GetActiveSlug(entity.Id, localeKeyGroup, language.Value.Id);
					if (value.HasValue())
					{
						dynamic exp = new ExpandoObject();
						exp.Culture = languageCulture;
						exp.LocaleKey = "SeName";
						exp.LocaleValue = value;

						localized.Add(exp);
					}
				}

				foreach (var keySelector in keySelectors)
				{
					var member = keySelector.Body as MemberExpression;
					var propInfo = member.Member as PropertyInfo;
					string localeKey = propInfo.Name;
					var value = _localizedEntityService.Value.GetLocalizedValue(language.Value.Id, entity.Id, localeKeyGroup, localeKey);

					// we better not export empty values. the risk is to high that they are imported and unnecessary fill databases.
					if (value.HasValue())
					{
						dynamic exp = new ExpandoObject();
						exp.Culture = languageCulture;
						exp.LocaleKey = localeKey;
						exp.LocaleValue = value;

						localized.Add(exp);
					}
				}
			}

			return (localized.Count == 0 ? null : localized);
		}

		private IExportSegmenterProvider CreateSegmenter(ExportProfileTaskContext ctx, int pageIndex = 0)
		{
			var offset = ctx.Profile.Offset + (pageIndex * PageSize);

			var limit = (ctx.IsPreview ? PageSize : ctx.Profile.Limit);
			
			var recordsPerSegment = (ctx.IsPreview ? 0 : ctx.Profile.BatchSize);
			
			var totalCount = ctx.Profile.Offset + ctx.RecordsPerStore.First(x => x.Key == ctx.Store.Id).Value;

			switch (ctx.Provider.Value.EntityType)
			{
				case ExportEntityType.Product:
					ctx.Export.Segmenter = new ExportSegmenter<Product>
					(
						skip => GetProducts(ctx, skip),
						entities =>
						{
							// load data behind navigation properties for current queue in one go
							ctx.DataContextProduct = new ExportDataContextProduct(entities,
								x => _productAttributeService.Value.GetProductVariantAttributesByProductIds(x, null),
								x => _productAttributeService.Value.GetProductVariantAttributeCombinations(x),
								x => _productService.Value.GetTierPricesByProductIds(x, (ctx.Projection.CurrencyId ?? 0) != 0 ? ctx.ContextCustomer : null, ctx.Store.Id),
								x => _categoryService.Value.GetProductCategoriesByProductIds(x),
								x => _manufacturerService.Value.GetProductManufacturersByProductIds(x),
								x => _productService.Value.GetProductPicturesByProductIds(x),
								x => _productService.Value.GetProductTagsByProductIds(x),
								x => _productService.Value.GetAppliedDiscountsByProductIds(x),
								x => _productService.Value.GetProductSpecificationAttributesByProductIds(x),
								x => _productService.Value.GetBundleItemsByProductIds(x, true)
							);
						},
						entity => ConvertToExpando(ctx, entity),
						offset, PageSize, limit, recordsPerSegment, totalCount
					);
					break;

				case ExportEntityType.Order:
					ctx.Export.Segmenter = new ExportSegmenter<Order>
					(
						skip => GetOrders(ctx, skip),
						entities =>
						{
							ctx.DataContextOrder = new ExportDataContextOrder(entities,
								x => _customerService.Value.GetCustomersByIds(x),
								x => _customerService.Value.GetRewardPointsHistoriesByCustomerIds(x),
								x => _addressesService.Value.GetAddressByIds(x),
								x => _orderService.Value.GetOrderItemsByOrderIds(x),
								x => _shipmentService.Value.GetShipmentsByOrderIds(x)
							);
						},
						entity => ConvertToExpando(ctx, entity),
						offset, PageSize, limit, recordsPerSegment, totalCount
					);
					break;

				case ExportEntityType.Manufacturer:
					ctx.Export.Segmenter = new ExportSegmenter<Manufacturer>
					(
						skip => GetManufacturers(ctx, skip),
						entities =>
						{
							ctx.DataContextManufacturer = new ExportDataContextManufacturer(entities,
								x => _manufacturerService.Value.GetProductManufacturersByManufacturerIds(x),
								x => _pictureService.Value.GetPicturesByIds(x)
							);
						},
						entity => ConvertToExpando(ctx, entity),
						offset, PageSize, limit, recordsPerSegment, totalCount
					);
					break;

				case ExportEntityType.Category:
					ctx.Export.Segmenter = new ExportSegmenter<Category>
					(
						skip => GetCategories(ctx, skip),
						entities =>
						{
							ctx.DataContextCategory = new ExportDataContextCategory(entities,
								x => _categoryService.Value.GetProductCategoriesByCategoryIds(x),
								x => _pictureService.Value.GetPicturesByIds(x)
							);
						},
						entity => ConvertToExpando(ctx, entity),
						offset, PageSize, limit, recordsPerSegment, totalCount
					);
					break;

				case ExportEntityType.Customer:
					ctx.Export.Segmenter = new ExportSegmenter<Customer>
					(
						skip => GetCustomers(ctx, skip),
						entities =>
						{
							ctx.DataContextCustomer = new ExportDataContextCustomer(entities,
								x => _genericAttributeService.Value.GetAttributesForEntity(x, "Customer")
							);
						},
						entity => ConvertToExpando(ctx, entity),
						offset, PageSize, limit, recordsPerSegment, totalCount
					);
					break;

				case ExportEntityType.NewsLetterSubscription:
					ctx.Export.Segmenter = new ExportSegmenter<NewsLetterSubscription>
					(
						skip => GetNewsLetterSubscriptions(ctx, skip),
						null,
						entity => ConvertToExpando(ctx, entity),
						offset, PageSize, limit, recordsPerSegment, totalCount
					);
					break;

				default:
					ctx.Export.Segmenter = null;
					break;
			}

			return ctx.Export.Segmenter as IExportSegmenterProvider;
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
						var productManus = ctx.DataContextProduct.ProductManufacturers.Load(product.Id);

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
					price = _taxService.Value.GetProductPrice(product, price.Value, true, ctx.ContextCustomer, out taxRate);
				}

				if (price != decimal.Zero)
				{
					price = _currencyService.Value.ConvertFromPrimaryStoreCurrency(price.Value, ctx.ContextCurrency, ctx.Store);
				}
			}
			return price;
		}

		private decimal CalculatePrice(ExportProfileTaskContext ctx, Product product, bool forAttributeCombination)
		{
			decimal price = product.Price;

			// price type
			if (ctx.Projection.PriceType.HasValue && !forAttributeCombination)
			{
				var priceCalculationContext = ctx.DataContextProduct as PriceCalculationContext;

				if (ctx.Projection.PriceType.Value == PriceDisplayType.LowestPrice)
				{
					bool displayFromMessage;
					price = _priceCalculationService.Value.GetLowestPrice(product, priceCalculationContext, out displayFromMessage);
				}
				else if (ctx.Projection.PriceType.Value == PriceDisplayType.PreSelectedPrice)
				{
					price = _priceCalculationService.Value.GetPreselectedPrice(product, priceCalculationContext);
				}
				else if (ctx.Projection.PriceType.Value == PriceDisplayType.PriceWithoutDiscountsAndAttributes)
				{
					price = _priceCalculationService.Value.GetFinalPrice(product, null, ctx.ContextCustomer, decimal.Zero, false, 1, null, priceCalculationContext);
				}
			}

			return ConvertPrice(ctx, product, price) ?? price;
		}

		private void GetDeliveryTimeAndQuantityUnit(ExportProfileTaskContext ctx, dynamic expando, int? deliveryTimeId, int? quantityUnitId)
		{
			if (ctx.DeliveryTimes != null)
			{
				if (deliveryTimeId.HasValue && ctx.DeliveryTimes.ContainsKey(deliveryTimeId.Value))
					expando.DeliveryTime = ToExpando(ctx, ctx.DeliveryTimes[deliveryTimeId.Value]);
				else
					expando.DeliveryTime = null;
			}

			if (ctx.QuantityUnits != null)
			{
				if (quantityUnitId.HasValue && ctx.QuantityUnits.ContainsKey(quantityUnitId.Value))
					expando.QuantityUnit = ToExpando(ctx, ctx.QuantityUnits[quantityUnitId.Value]);
				else
					expando.QuantityUnit = null;
			}
		}

		private void SetProgress(ExportProfileTaskContext ctx, int loadedRecords)
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

		private void SetProgress(ExportProfileTaskContext ctx, string message)
		{
			if (!ctx.IsPreview && ctx.TaskContext.ScheduleTask != null && message.HasValue())
			{
				ctx.TaskContext.SetProgress(null, message, true);
			}
		}

		private void SendCompletionEmail(ExportProfileTaskContext ctx)
		{
			var emailAccount = _emailAccountService.Value.GetEmailAccountById(ctx.Profile.EmailAccountId);
			var smtpContext = new SmtpContext(emailAccount);
			var message = new EmailMessage();

			var storeInfo = "{0} ({1})".FormatInvariant(ctx.Store.Name, ctx.Store.Url);

			message.To.AddRange(ctx.Profile.CompletedEmailAddresses.SplitSafe(",").Where(x => x.IsEmail()).Select(x => new EmailAddress(x)));
			message.From = new EmailAddress(emailAccount.Email, emailAccount.DisplayName);

			message.Subject = _services.Localization.GetResource("Admin.DataExchange.Export.CompletedEmail.Subject", ctx.Projection.LanguageId ?? 0)
				.FormatInvariant(ctx.Profile.Name);

			message.Body = _services.Localization.GetResource("Admin.DataExchange.Export.CompletedEmail.Body", ctx.Projection.LanguageId ?? 0)
				.FormatInvariant(storeInfo);

			_emailSender.Value.SendEmail(smtpContext, message);
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

				if (FileSystemHelper.Copy(ctx.ZipPath, path))
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
			var emailAccount = _emailAccountService.Value.GetEmailAccountById(deployment.EmailAccountId);
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

				_queuedEmailService.Value.InsertQueuedEmail(queuedEmail);
				++count;
			}

			ctx.Log.Information("{0} email(s) created and queued for deployment.".FormatInvariant(count));
		}

		private void DeployHttp(ExportProfileTaskContext ctx, ExportDeployment deployment)
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

					var response = client.PostAsync(url, formData).Result;

					if (response.IsSuccessStatusCode)
					{
						succeeded = count;
					}
					else if (response.Content != null)
					{
						var content = response.Content.ReadAsStringAsync().Result;

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
						webClient.UploadFile(url, path);

						++succeeded;
					}
				}
			}

			ctx.Log.Information("{0} file(s) successfully uploaded via HTTP ({1}).".FormatInvariant(succeeded, deployment.HttpTransmissionType.ToString()));
		}

		private void DeployFtp(ExportProfileTaskContext ctx, ExportDeployment deployment)
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

				var requestStream = request.GetRequestStream();

				using (var stream = new FileStream(path, FileMode.Open))
				{
					while ((bytesRead = stream.Read(buff, 0, buffLength)) != 0)
					{
						requestStream.Write(buff, 0, bytesRead);
					}
				}

				requestStream.Close();

				using (var response = (FtpWebResponse)request.GetResponse())
				{
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
			}

			ctx.Log.Information("{0} file(s) successfully uploaded via FTP.".FormatInvariant(succeededFiles));
		}

		#endregion

		#region Entity to expando

		private dynamic ToExpando(ExportProfileTaskContext ctx, Currency currency)
		{
			if (currency == null)
				return null;

			dynamic expando = new ExpandoObject();
			expando._Entity = currency;

			expando.Id = currency.Id;
			expando.Name = currency.GetLocalized(x => x.Name, ctx.Projection.LanguageId ?? 0, true, false);
			expando.CurrencyCode = currency.CurrencyCode;
			expando.Rate = currency.Rate;
			expando.DisplayLocale = currency.DisplayLocale;
			expando.CustomFormatting = currency.CustomFormatting;
			expando.LimitedToStores = currency.LimitedToStores;
			expando.Published = currency.Published;
			expando.DisplayOrder = currency.DisplayOrder;
			expando.CreatedOnUtc = currency.CreatedOnUtc;
			expando.UpdatedOnUtc = currency.UpdatedOnUtc;
			expando.DomainEndings = currency.DomainEndings;

			expando._Localized = GetLocalized(ctx, currency, x => x.Name);

			return expando;
		}

		private dynamic ToExpando(ExportProfileTaskContext ctx, Language language)
		{
			if (language == null)
				return null;

			dynamic expando = new ExpandoObject();
			expando._Entity = language;

			expando.Id = language.Id;
			expando.Name = language.Name;
			expando.LanguageCulture = language.LanguageCulture;
			expando.UniqueSeoCode = language.UniqueSeoCode;
			expando.FlagImageFileName = language.FlagImageFileName;
			expando.Rtl = language.Rtl;
			expando.LimitedToStores = language.LimitedToStores;
			expando.Published = language.Published;
			expando.DisplayOrder = language.DisplayOrder;

			return expando;
		}

		private dynamic ToExpando(ExportProfileTaskContext ctx, Country country)
		{
			if (country == null)
				return null;

			dynamic expando = new ExpandoObject();
			expando._Entity = country;

			expando.Id = country.Id;
			expando.Name = country.GetLocalized(x => x.Name, ctx.Projection.LanguageId ?? 0, true, false);
			expando.AllowsBilling = country.AllowsBilling;
			expando.AllowsShipping = country.AllowsShipping;
			expando.TwoLetterIsoCode = country.TwoLetterIsoCode;
			expando.ThreeLetterIsoCode = country.ThreeLetterIsoCode;
			expando.NumericIsoCode = country.NumericIsoCode;
			expando.SubjectToVat = country.SubjectToVat;
			expando.Published = country.Published;
			expando.DisplayOrder = country.DisplayOrder;
			expando.LimitedToStores = country.LimitedToStores;

			expando._Localized = GetLocalized(ctx, country, x => x.Name);

			return expando;
		}

		private dynamic ToExpando(ExportProfileTaskContext ctx, Address address)
		{
			if (address == null)
				return null;

			dynamic expando = new ExpandoObject();
			expando._Entity = address;

			expando.Id = address.Id;
			expando.FirstName = address.FirstName;
			expando.LastName = address.LastName;
			expando.Email = address.Email;
			expando.Company = address.Company;
			expando.CountryId = address.CountryId;
			expando.StateProvinceId = address.StateProvinceId;
			expando.City = address.City;
			expando.Address1 = address.Address1;
			expando.Address2 = address.Address2;
			expando.ZipPostalCode = address.ZipPostalCode;
			expando.PhoneNumber = address.PhoneNumber;
			expando.FaxNumber = address.FaxNumber;
			expando.CreatedOnUtc = address.CreatedOnUtc;

			expando.Country = ToExpando(ctx, address.Country);

			if (address.StateProvince != null)
			{
				dynamic sp = new ExpandoObject();
				sp._Entity = address.StateProvince;
				sp.Id = address.StateProvince.Id;
				sp.CountryId = address.StateProvince.CountryId;
				sp.Name = address.StateProvince.GetLocalized(x => x.Name, ctx.Projection.LanguageId ?? 0, true, false);
				sp.Abbreviation = address.StateProvince.Abbreviation;
				sp.Published = address.StateProvince.Published;
				sp.DisplayOrder = address.StateProvince.DisplayOrder;

				sp._Localized = GetLocalized(ctx, address.StateProvince, x => x.Name);

				expando.StateProvince = sp;
			}
			else
			{
				expando.StateProvince = null;
			}

			return expando;
		}

		private dynamic ToExpando(ExportProfileTaskContext ctx, RewardPointsHistory points)
		{
			if (points == null)
				return null;

			dynamic expando = new ExpandoObject();
			expando._Entity = points;

			expando.Id = points.Id;
			expando.CustomerId = points.CustomerId;
			expando.Points = points.Points;
			expando.PointsBalance = points.PointsBalance;
			expando.UsedAmount = points.UsedAmount;
			expando.Message = points.Message;
			expando.CreatedOnUtc = points.CreatedOnUtc;

			return expando;
		}

		private dynamic ToExpando(ExportProfileTaskContext ctx, Customer customer)
		{
			if (customer == null)
				return null;

			dynamic expando = new ExpandoObject();
			expando._Entity = customer;

			expando.Id = customer.Id;
			expando.CustomerGuid = customer.CustomerGuid;
			expando.Username = customer.Username;
			expando.Email = customer.Email;
			expando.Password = customer.Password;	// not so good but referenced in Excel export
			expando.PasswordFormatId = customer.PasswordFormatId;
			expando.PasswordSalt = customer.PasswordSalt;
			expando.AdminComment = customer.AdminComment;
			expando.IsTaxExempt = customer.IsTaxExempt;
			expando.AffiliateId = customer.AffiliateId;
			expando.Active = customer.Active;
			expando.Deleted = customer.Deleted;
			expando.IsSystemAccount = customer.IsSystemAccount;
			expando.SystemName = customer.SystemName;
			expando.LastIpAddress = customer.LastIpAddress;
			expando.CreatedOnUtc = customer.CreatedOnUtc;
			expando.LastLoginDateUtc = customer.LastLoginDateUtc;
			expando.LastActivityDateUtc = customer.LastActivityDateUtc;

			expando.BillingAddress = null;
			expando.ShippingAddress = null;
			expando.Addresses = null;
			expando.CustomerRoles = null;

			expando.RewardPointsHistory = null;
			expando._RewardPointsBalance = 0;

			expando._GenericAttributes = null;
			expando._HasNewsletterSubscription = false;

			return expando;
		}

		private dynamic ToExpando(ExportProfileTaskContext ctx, Store store)
		{
			if (store == null)
				return null;

			dynamic expando = new ExpandoObject();
			expando._Entity = store;

			expando.Id = store.Id;
			expando.Name = store.Name;
			expando.Url = store.Url;
			expando.SslEnabled = store.SslEnabled;
			expando.SecureUrl = store.SecureUrl;
			expando.Hosts = store.Hosts;
			expando.LogoPictureId = store.LogoPictureId;
			expando.DisplayOrder = store.DisplayOrder;
			expando.HtmlBodyId = store.HtmlBodyId;
			expando.ContentDeliveryNetwork = store.ContentDeliveryNetwork;
			expando.PrimaryStoreCurrencyId = store.PrimaryStoreCurrencyId;
			expando.PrimaryExchangeRateCurrencyId = store.PrimaryExchangeRateCurrencyId;

			expando.PrimaryStoreCurrency = ToExpando(ctx, store.PrimaryStoreCurrency);
			expando.PrimaryExchangeRateCurrency = ToExpando(ctx, store.PrimaryExchangeRateCurrency);

			return expando;
		}

		private dynamic ToExpando(ExportProfileTaskContext ctx, DeliveryTime deliveryTime)
		{
			if (deliveryTime == null)
				return null;		

			dynamic expando = new ExpandoObject();
			expando._Entity = deliveryTime;

			expando.Id = deliveryTime.Id;
			expando.Name = deliveryTime.GetLocalized(x => x.Name, ctx.Projection.LanguageId ?? 0, true, false);
			expando.DisplayLocale = deliveryTime.DisplayLocale;
			expando.ColorHexValue = deliveryTime.ColorHexValue;
			expando.DisplayOrder = deliveryTime.DisplayOrder;

			expando._Localized = GetLocalized(ctx, deliveryTime, x => x.Name);

			return expando;
		}

		private dynamic ToExpando(ExportProfileTaskContext ctx, QuantityUnit quantityUnit)
		{
			if (quantityUnit == null)
				return null;

			dynamic expando = new ExpandoObject();
			expando._Entity = quantityUnit;

			expando.Id = quantityUnit.Id;
			expando.Name = quantityUnit.GetLocalized(x => x.Name, ctx.Projection.LanguageId ?? 0, true, false);
			expando.Description = quantityUnit.GetLocalized(x => x.Description, ctx.Projection.LanguageId ?? 0, true, false);
			expando.DisplayLocale = quantityUnit.DisplayLocale;
			expando.DisplayOrder = quantityUnit.DisplayOrder;
			expando.IsDefault = quantityUnit.IsDefault;

			expando._Localized = GetLocalized(ctx, quantityUnit,
				x => x.Name,
				x => x.Description);

			return expando;
		}

		private dynamic ToExpando(ExportProfileTaskContext ctx, Picture picture, int thumbPictureSize, int detailsPictureSize)
		{
			if (picture == null)
				return null;

			dynamic expando = new ExpandoObject();
			expando._Entity = picture;

			expando.Id = picture.Id;
			expando.SeoFilename = picture.SeoFilename;
			expando.MimeType = picture.MimeType;

			expando._ThumbImageUrl = _pictureService.Value.GetPictureUrl(picture, thumbPictureSize, false, ctx.Store.Url);
			expando._ImageUrl = _pictureService.Value.GetPictureUrl(picture, detailsPictureSize, false, ctx.Store.Url);
			expando._FullSizeImageUrl = _pictureService.Value.GetPictureUrl(picture, 0, false, ctx.Store.Url);

			var relativeUrl = _pictureService.Value.GetPictureUrl(picture);
			expando._FileName = relativeUrl.Substring(relativeUrl.LastIndexOf("/") + 1);

			expando._ThumbLocalPath = _pictureService.Value.GetThumbLocalPath(picture);

			return expando;
		}

		private dynamic ToExpando(ExportProfileTaskContext ctx, ProductVariantAttribute pva)
		{
			if (pva == null)
				return null;

			dynamic expando = new ExpandoObject();
			expando._Entity = pva;

			expando.Id = pva.Id;
			expando.TextPrompt = pva.TextPrompt;
			expando.IsRequired = pva.IsRequired;
			expando.AttributeControlTypeId = pva.AttributeControlTypeId;
			expando.DisplayOrder = pva.DisplayOrder;

			dynamic attribute = new ExpandoObject();
			attribute._Entity = pva.ProductAttribute;
			attribute.Id = pva.ProductAttribute.Id;
			attribute.Alias = pva.ProductAttribute.Alias;
			attribute.Name = pva.ProductAttribute.GetLocalized(x => x.Name, ctx.Projection.LanguageId ?? 0, true, false);
			attribute.Description = pva.ProductAttribute.GetLocalized(x => x.Description, ctx.Projection.LanguageId ?? 0, true, false);

			attribute.Values = pva.ProductVariantAttributeValues
				.OrderBy(x => x.DisplayOrder)
				.Select(x =>
				{
					dynamic value = new ExpandoObject();
					value._Entity = x;
					value.Id = x.Id;
					value.Alias = x.Alias;
					value.Name = x.GetLocalized(y => y.Name, ctx.Projection.LanguageId ?? 0, true, false);
					value.ColorSquaresRgb = x.ColorSquaresRgb;
					value.PriceAdjustment = x.PriceAdjustment;
					value.WeightAdjustment = x.WeightAdjustment;
					value.IsPreSelected = x.IsPreSelected;
					value.DisplayOrder = x.DisplayOrder;
					value.ValueTypeId = x.ValueTypeId;
					value.LinkedProductId = x.LinkedProductId;
					value.Quantity = x.Quantity;

					value._Localized = GetLocalized(ctx, x, y => y.Name);

					return value;
				})
				.ToList();

			attribute._Localized = GetLocalized(ctx, pva.ProductAttribute,
				x => x.Name,
				x => x.Description);

			expando.Attribute = attribute;

			return expando;
		}

		private dynamic ToExpando(ExportProfileTaskContext ctx, ProductVariantAttributeCombination pvac)
		{
			if (pvac == null)
				return null;

			dynamic expando = new ExpandoObject();
			expando._Entity = pvac;

			expando.Id = pvac.Id;
			expando.StockQuantity = pvac.StockQuantity;
			expando.AllowOutOfStockOrders = pvac.AllowOutOfStockOrders;
			expando.AttributesXml = pvac.AttributesXml;
			expando.Sku = pvac.Sku;
			expando.Gtin = pvac.Gtin;
			expando.ManufacturerPartNumber = pvac.ManufacturerPartNumber;
			expando.Price = pvac.Price;
			expando.Length = pvac.Length;
			expando.Width = pvac.Width;
			expando.Height = pvac.Height;
			expando.BasePriceAmount = pvac.BasePriceAmount;
			expando.BasePriceBaseAmount = pvac.BasePriceBaseAmount;
			expando.AssignedPictureIds = pvac.AssignedPictureIds;
			expando.DeliveryTimeId = pvac.DeliveryTimeId;
			expando.IsActive = pvac.IsActive;

			GetDeliveryTimeAndQuantityUnit(ctx, expando, pvac.DeliveryTimeId, pvac.QuantityUnitId);

			return expando;
		}

		private dynamic ToExpando(ExportProfileTaskContext ctx, Manufacturer manufacturer)
		{
			if (manufacturer == null)
				return null;

			dynamic expando = new ExpandoObject();
			expando._Entity = manufacturer;

			expando.Id = manufacturer.Id;
			expando.Name = manufacturer.GetLocalized(x => x.Name, ctx.Projection.LanguageId ?? 0, true, false);
			expando.SeName = manufacturer.GetSeName(ctx.Projection.LanguageId ?? 0, true, false);
			expando.Description = manufacturer.GetLocalized(x => x.Description, ctx.Projection.LanguageId ?? 0, true, false);
			expando.ManufacturerTemplateId = manufacturer.ManufacturerTemplateId;
			expando.MetaKeywords = manufacturer.GetLocalized(x => x.MetaKeywords, ctx.Projection.LanguageId ?? 0, true, false);
			expando.MetaDescription = manufacturer.GetLocalized(x => x.MetaDescription, ctx.Projection.LanguageId ?? 0, true, false);
			expando.MetaTitle = manufacturer.GetLocalized(x => x.MetaTitle, ctx.Projection.LanguageId ?? 0, true, false);
			expando.PictureId = manufacturer.PictureId;
			expando.PageSize = manufacturer.PageSize;
			expando.AllowCustomersToSelectPageSize = manufacturer.AllowCustomersToSelectPageSize;
			expando.PageSizeOptions = manufacturer.PageSizeOptions;
			expando.PriceRanges = manufacturer.PriceRanges;
			expando.Published = manufacturer.Published;
			expando.Deleted = manufacturer.Deleted;
			expando.DisplayOrder = manufacturer.DisplayOrder;
			expando.CreatedOnUtc = manufacturer.CreatedOnUtc;
			expando.UpdatedOnUtc = manufacturer.UpdatedOnUtc;

			expando.Picture = null;

			expando._Localized = GetLocalized(ctx, manufacturer,
				x => x.Name,
				x => x.Description,
				x => x.MetaKeywords,
				x => x.MetaDescription,
				x => x.MetaTitle);

			return expando;
		}

		private dynamic ToExpando(ExportProfileTaskContext ctx, Category category)
		{
			if (category == null)
				return null;

			dynamic expando = new ExpandoObject();
			expando._Entity = category;

			expando.Id = category.Id;
			expando.Name = category.GetLocalized(x => x.Name, ctx.Projection.LanguageId ?? 0, true, false);
			expando.SeName = category.GetSeName(ctx.Projection.LanguageId ?? 0, true, false);
			expando.FullName = category.GetLocalized(x => x.FullName, ctx.Projection.LanguageId ?? 0, true, false);
			expando.Description = category.GetLocalized(x => x.Description, ctx.Projection.LanguageId ?? 0, true, false);
			expando.BottomDescription = category.GetLocalized(x => x.BottomDescription, ctx.Projection.LanguageId ?? 0, true, false);
			expando.CategoryTemplateId = category.CategoryTemplateId;
			expando.MetaKeywords = category.GetLocalized(x => x.MetaKeywords, ctx.Projection.LanguageId ?? 0, true, false);
			expando.MetaDescription = category.GetLocalized(x => x.MetaDescription, ctx.Projection.LanguageId ?? 0, true, false);
			expando.MetaTitle = category.GetLocalized(x => x.MetaTitle, ctx.Projection.LanguageId ?? 0, true, false);
			expando.ParentCategoryId = category.ParentCategoryId;
			expando.PictureId = category.PictureId;
			expando.PageSize = category.PageSize;
			expando.AllowCustomersToSelectPageSize = category.AllowCustomersToSelectPageSize;
			expando.PageSizeOptions = category.PageSizeOptions;
			expando.PriceRanges = category.PriceRanges;
			expando.ShowOnHomePage = category.ShowOnHomePage;
			expando.HasDiscountsApplied = category.HasDiscountsApplied;
			expando.Published = category.Published;
			expando.Deleted = category.Deleted;
			expando.DisplayOrder = category.DisplayOrder;
			expando.CreatedOnUtc = category.CreatedOnUtc;
			expando.UpdatedOnUtc = category.UpdatedOnUtc;
			expando.SubjectToAcl = category.SubjectToAcl;
			expando.LimitedToStores = category.LimitedToStores;
			expando.Alias = category.Alias;
			expando.DefaultViewMode = category.DefaultViewMode;

			expando.Picture = null;

			expando._Localized = GetLocalized(ctx, category,
				x => x.Name,
				x => x.FullName,
				x => x.Description,
				x => x.BottomDescription,
				x => x.MetaKeywords,
				x => x.MetaDescription,
				x => x.MetaTitle);

			return expando;
		}

		private dynamic ToExpando(ExportProfileTaskContext ctx, Product product)
		{
			if (product == null)
				return null;

			dynamic expando = new ExpandoObject();
			expando._Entity = product;

			expando.Id = product.Id;
			expando.ProductTypeId = product.ProductTypeId;
			expando.ParentGroupedProductId = product.ParentGroupedProductId;
			expando.VisibleIndividually = product.VisibleIndividually;
			expando.Name = product.GetLocalized(x => x.Name, ctx.Projection.LanguageId ?? 0, true, false);
			expando.SeName = product.GetSeName(ctx.Projection.LanguageId ?? 0, true, false);
			expando.ShortDescription = product.GetLocalized(x => x.ShortDescription, ctx.Projection.LanguageId ?? 0, true, false);
			expando.FullDescription = product.GetLocalized(x => x.FullDescription, ctx.Projection.LanguageId ?? 0, true, false);
			expando.AdminComment = product.AdminComment;
			expando.ProductTemplateId = product.ProductTemplateId;
			expando.ShowOnHomePage = product.ShowOnHomePage;
			expando.HomePageDisplayOrder = product.HomePageDisplayOrder;
			expando.MetaKeywords = product.GetLocalized(x => x.MetaKeywords, ctx.Projection.LanguageId ?? 0, true, false);
			expando.MetaDescription = product.GetLocalized(x => x.MetaDescription, ctx.Projection.LanguageId ?? 0, true, false);
			expando.MetaTitle = product.GetLocalized(x => x.MetaTitle, ctx.Projection.LanguageId ?? 0, true, false);
			expando.AllowCustomerReviews = product.AllowCustomerReviews;
			expando.ApprovedRatingSum = product.ApprovedRatingSum;
			expando.NotApprovedRatingSum = product.NotApprovedRatingSum;
			expando.ApprovedTotalReviews = product.ApprovedTotalReviews;
			expando.NotApprovedTotalReviews = product.NotApprovedTotalReviews;
			expando.SubjectToAcl = product.SubjectToAcl;
			expando.LimitedToStores = product.LimitedToStores;
			expando.Sku = product.Sku;
			expando.ManufacturerPartNumber = product.ManufacturerPartNumber;
			expando.Gtin = product.Gtin;
			expando.IsGiftCard = product.IsGiftCard;
			expando.GiftCardTypeId = product.GiftCardTypeId;
			expando.RequireOtherProducts = product.RequireOtherProducts;
			expando.RequiredProductIds = product.RequiredProductIds;
			expando.AutomaticallyAddRequiredProducts = product.AutomaticallyAddRequiredProducts;
			expando.IsDownload = product.IsDownload;
			expando.DownloadId = product.DownloadId;
			expando.UnlimitedDownloads = product.UnlimitedDownloads;
			expando.MaxNumberOfDownloads = product.MaxNumberOfDownloads;
			expando.DownloadExpirationDays = product.DownloadExpirationDays;
			expando.DownloadActivationTypeId = product.DownloadActivationTypeId;
			expando.HasSampleDownload = product.HasSampleDownload;
			expando.SampleDownloadId = product.SampleDownloadId;
			expando.HasUserAgreement = product.HasUserAgreement;
			expando.UserAgreementText = product.UserAgreementText;
			expando.IsRecurring = product.IsRecurring;
			expando.RecurringCycleLength = product.RecurringCycleLength;
			expando.RecurringCyclePeriodId = product.RecurringCyclePeriodId;
			expando.RecurringTotalCycles = product.RecurringTotalCycles;
			expando.IsShipEnabled = product.IsShipEnabled;
			expando.IsFreeShipping = product.IsFreeShipping;
			expando.AdditionalShippingCharge = product.AdditionalShippingCharge;
			expando.IsTaxExempt = product.IsTaxExempt;
			expando.IsEsd = product.IsEsd;
			expando.TaxCategoryId = product.TaxCategoryId;
			expando.ManageInventoryMethodId = product.ManageInventoryMethodId;
			expando.StockQuantity = product.StockQuantity;
			expando.DisplayStockAvailability = product.DisplayStockAvailability;
			expando.DisplayStockQuantity = product.DisplayStockQuantity;
			expando.MinStockQuantity = product.MinStockQuantity;
			expando.LowStockActivityId = product.LowStockActivityId;
			expando.NotifyAdminForQuantityBelow = product.NotifyAdminForQuantityBelow;
			expando.BackorderModeId = product.BackorderModeId;
			expando.AllowBackInStockSubscriptions = product.AllowBackInStockSubscriptions;
			expando.OrderMinimumQuantity = product.OrderMinimumQuantity;
			expando.OrderMaximumQuantity = product.OrderMaximumQuantity;
			expando.AllowedQuantities = product.AllowedQuantities;
			expando.DisableBuyButton = product.DisableBuyButton;
			expando.DisableWishlistButton = product.DisableWishlistButton;
			expando.AvailableForPreOrder = product.AvailableForPreOrder;
			expando.CallForPrice = product.CallForPrice;
			expando.Price = product.Price;
			expando.OldPrice = product.OldPrice;
			expando.ProductCost = product.ProductCost;
			expando.SpecialPrice = product.SpecialPrice;
			expando.SpecialPriceStartDateTimeUtc = product.SpecialPriceStartDateTimeUtc;
			expando.SpecialPriceEndDateTimeUtc = product.SpecialPriceEndDateTimeUtc;
			expando.CustomerEntersPrice = product.CustomerEntersPrice;
			expando.MinimumCustomerEnteredPrice = product.MinimumCustomerEnteredPrice;
			expando.MaximumCustomerEnteredPrice = product.MaximumCustomerEnteredPrice;
			expando.HasTierPrices = product.HasTierPrices;
			expando.LowestAttributeCombinationPrice = product.LowestAttributeCombinationPrice;
			expando.HasDiscountsApplied = product.HasDiscountsApplied;
			expando.Weight = product.Weight;
			expando.Length = product.Length;
			expando.Width = product.Width;
			expando.Height = product.Height;
			expando.AvailableStartDateTimeUtc = product.AvailableStartDateTimeUtc;
			expando.AvailableEndDateTimeUtc = product.AvailableEndDateTimeUtc;
			expando.DisplayOrder = product.DisplayOrder;
			expando.Published = product.Published;
			expando.Deleted = product.Deleted;
			expando.CreatedOnUtc = product.CreatedOnUtc;
			expando.UpdatedOnUtc = product.UpdatedOnUtc;
			expando.DeliveryTimeId = product.DeliveryTimeId;
			expando.QuantityUnitId = product.QuantityUnitId;
			expando.BasePriceEnabled = product.BasePriceEnabled;
			expando.BasePriceMeasureUnit = product.BasePriceMeasureUnit;
			expando.BasePriceAmount = product.BasePriceAmount;
			expando.BasePriceBaseAmount = product.BasePriceBaseAmount;
			expando.BasePriceHasValue = product.BasePriceHasValue;
			expando.BundleTitleText = product.GetLocalized(x => x.BundleTitleText, ctx.Projection.LanguageId ?? 0, true, false);
			expando.BundlePerItemShipping = product.BundlePerItemShipping;
			expando.BundlePerItemPricing = product.BundlePerItemPricing;
			expando.BundlePerItemShoppingCart = product.BundlePerItemShoppingCart;

			expando.AppliedDiscounts = null;
			expando.TierPrices = null;
			expando.ProductAttributes = null;
			expando.ProductAttributeCombinations = null;
			expando.ProductPictures = null;
			expando.ProductCategories = null;
			expando.ProductManufacturers = null;
			expando.ProductTags = null;
			expando.ProductSpecificationAttributes = null;
			expando.ProductBundleItems = null;

			expando._Localized = GetLocalized(ctx, product,
				x => x.Name,
				x => x.ShortDescription,
				x => x.FullDescription,
				x => x.MetaKeywords,
				x => x.MetaDescription,
				x => x.MetaTitle,
				x => x.BundleTitleText);

			return expando;
		}

		private dynamic ToExpando(ExportProfileTaskContext ctx, Order order)
		{
			if (order == null)
				return null;

			dynamic expando = new ExpandoObject();
			expando._Entity = order;

			expando.Id = order.Id;
			expando.OrderNumber = order.GetOrderNumber();
			expando.OrderGuid = order.OrderGuid;
			expando.StoreId = order.StoreId;
			expando.CustomerId = order.CustomerId;
			expando.BillingAddressId = order.BillingAddressId;
			expando.ShippingAddressId = order.ShippingAddressId;
			expando.OrderStatusId = order.OrderStatusId;
			expando.ShippingStatusId = order.ShippingStatusId;
			expando.PaymentStatusId = order.PaymentStatusId;
			expando.PaymentMethodSystemName = order.PaymentMethodSystemName;
			expando.CustomerCurrencyCode = order.CustomerCurrencyCode;
			expando.CurrencyRate = order.CurrencyRate;
			expando.CustomerTaxDisplayTypeId = order.CustomerTaxDisplayTypeId;
			expando.VatNumber = order.VatNumber;
			expando.OrderSubtotalInclTax = order.OrderSubtotalInclTax;
			expando.OrderSubtotalExclTax = order.OrderSubtotalExclTax;
			expando.OrderSubTotalDiscountInclTax = order.OrderSubTotalDiscountInclTax;
			expando.OrderSubTotalDiscountExclTax = order.OrderSubTotalDiscountExclTax;
			expando.OrderShippingInclTax = order.OrderShippingInclTax;
			expando.OrderShippingExclTax = order.OrderShippingExclTax;
			expando.OrderShippingTaxRate = order.OrderShippingTaxRate;
			expando.PaymentMethodAdditionalFeeInclTax = order.PaymentMethodAdditionalFeeInclTax;
			expando.PaymentMethodAdditionalFeeExclTax = order.PaymentMethodAdditionalFeeExclTax;
			expando.PaymentMethodAdditionalFeeTaxRate = order.PaymentMethodAdditionalFeeTaxRate;
			expando.TaxRates = order.TaxRates;
			expando.OrderTax = order.OrderTax;
			expando.OrderDiscount = order.OrderDiscount;
			expando.OrderTotal = order.OrderTotal;
			expando.RefundedAmount = order.RefundedAmount;
			expando.RewardPointsWereAdded = order.RewardPointsWereAdded;
			expando.CheckoutAttributeDescription = order.CheckoutAttributeDescription;
			expando.CheckoutAttributesXml = order.CheckoutAttributesXml;
			expando.CustomerLanguageId = order.CustomerLanguageId;
			expando.AffiliateId = order.AffiliateId;
			expando.CustomerIp = order.CustomerIp;
			expando.AllowStoringCreditCardNumber = order.AllowStoringCreditCardNumber;
			expando.CardType = order.CardType;
			expando.CardName = order.CardName;
			expando.CardNumber = order.CardNumber;
			expando.MaskedCreditCardNumber = order.MaskedCreditCardNumber;
			expando.CardCvv2 = order.CardCvv2;
			expando.CardExpirationMonth = order.CardExpirationMonth;
			expando.CardExpirationYear = order.CardExpirationYear;
			expando.AllowStoringDirectDebit = order.AllowStoringDirectDebit;
			expando.DirectDebitAccountHolder = order.DirectDebitAccountHolder;
			expando.DirectDebitAccountNumber = order.DirectDebitAccountNumber;
			expando.DirectDebitBankCode = order.DirectDebitBankCode;
			expando.DirectDebitBankName = order.DirectDebitBankName;
			expando.DirectDebitBIC = order.DirectDebitBIC;
			expando.DirectDebitCountry = order.DirectDebitCountry;
			expando.DirectDebitIban = order.DirectDebitIban;
			expando.CustomerOrderComment = order.CustomerOrderComment;
			expando.AuthorizationTransactionId = order.AuthorizationTransactionId;
			expando.AuthorizationTransactionCode = order.AuthorizationTransactionCode;
			expando.AuthorizationTransactionResult = order.AuthorizationTransactionResult;
			expando.CaptureTransactionId = order.CaptureTransactionId;
			expando.CaptureTransactionResult = order.CaptureTransactionResult;
			expando.SubscriptionTransactionId = order.SubscriptionTransactionId;
			expando.PurchaseOrderNumber = order.PurchaseOrderNumber;
			expando.PaidDateUtc = order.PaidDateUtc;
			expando.ShippingMethod = order.ShippingMethod;
			expando.ShippingRateComputationMethodSystemName = order.ShippingRateComputationMethodSystemName;
			expando.Deleted = order.Deleted;
			expando.CreatedOnUtc = order.CreatedOnUtc;
			expando.UpdatedOnUtc = order.UpdatedOnUtc;
			expando.RewardPointsRemaining = order.RewardPointsRemaining;
			expando.HasNewPaymentNotification = order.HasNewPaymentNotification;
			expando.OrderStatus = order.OrderStatus.GetLocalizedEnum(_services.Localization, ctx.Projection.LanguageId ?? 0);
			expando.PaymentStatus = order.PaymentStatus.GetLocalizedEnum(_services.Localization, ctx.Projection.LanguageId ?? 0);
			expando.ShippingStatus = order.ShippingStatus.GetLocalizedEnum(_services.Localization, ctx.Projection.LanguageId ?? 0);

			expando.Customer = null;
			expando.BillingAddress = null;
			expando.ShippingAddress = null;
			expando.Store = null;
			expando.Shipments = null;
			expando.RedeemedRewardPointsEntry = ToExpando(ctx, order.RedeemedRewardPointsEntry);

			return expando;
		}

		private dynamic ToExpando(ExportProfileTaskContext ctx, OrderItem orderItem)
		{
			if (orderItem == null)
				return null;

			dynamic expando = new ExpandoObject();
			expando._Entity = orderItem;

			expando.Id = orderItem.Id;
			expando.OrderItemGuid = orderItem.OrderItemGuid;
			expando.OrderId = orderItem.OrderId;
			expando.ProductId = orderItem.ProductId;
			expando.Quantity = orderItem.Quantity;
			expando.UnitPriceInclTax = orderItem.UnitPriceInclTax;
			expando.UnitPriceExclTax = orderItem.UnitPriceExclTax;
			expando.PriceInclTax = orderItem.PriceInclTax;
			expando.PriceExclTax = orderItem.PriceExclTax;
			expando.TaxRate = orderItem.TaxRate;
			expando.DiscountAmountInclTax = orderItem.DiscountAmountInclTax;
			expando.DiscountAmountExclTax = orderItem.DiscountAmountExclTax;
			expando.AttributeDescription = orderItem.AttributeDescription;
			expando.AttributesXml = orderItem.AttributesXml;
			expando.DownloadCount = orderItem.DownloadCount;
			expando.IsDownloadActivated = orderItem.IsDownloadActivated;
			expando.LicenseDownloadId = orderItem.LicenseDownloadId;
			expando.ItemWeight = orderItem.ItemWeight;
			expando.BundleData = orderItem.BundleData;
			expando.ProductCost = orderItem.ProductCost;

			expando.Product = ToExpando(ctx, orderItem.Product);

			return expando;
		}

		private dynamic ToExpando(ExportProfileTaskContext ctx, Shipment shipment)
		{
			if (shipment == null)
				return null;

			dynamic expando = new ExpandoObject();
			expando._Entity = shipment;

			expando.Id = shipment.Id;
			expando.OrderId = shipment.OrderId;
			expando.TrackingNumber = shipment.TrackingNumber;
			expando.TotalWeight = shipment.TotalWeight;
			expando.ShippedDateUtc = shipment.ShippedDateUtc;
			expando.DeliveryDateUtc = shipment.DeliveryDateUtc;
			expando.CreatedOnUtc = shipment.CreatedOnUtc;

			expando.ShipmentItems = shipment.ShipmentItems
				.Select(x =>
				{
					dynamic exp = new ExpandoObject();
					exp._Entity = x;
					exp.Id = x.Id;
					exp.ShipmentId = x.ShipmentId;
					exp.OrderItemId = x.OrderItemId;
					exp.Quantity = x.Quantity;
					return exp;
				})
				.ToList();

			return expando;
		}

		private dynamic ToExpando(ExportProfileTaskContext ctx, Discount discount)
		{
			if (discount == null)
				return null;

			dynamic expando = new ExpandoObject();
			expando._Entity = discount;

			expando.Id = discount.Id;
			expando.Name = discount.Name;
			expando.DiscountTypeId = discount.DiscountTypeId;
			expando.UsePercentage = discount.UsePercentage;
			expando.DiscountPercentage = discount.DiscountPercentage;
			expando.DiscountAmount = discount.DiscountAmount;
			expando.StartDateUtc = discount.StartDateUtc;
			expando.EndDateUtc = discount.EndDateUtc;
			expando.RequiresCouponCode = discount.RequiresCouponCode;
			expando.CouponCode = discount.CouponCode;
			expando.DiscountLimitationId = discount.DiscountLimitationId;
			expando.LimitationTimes = discount.LimitationTimes;

			return expando;
		}

		private dynamic ToExpando(ExportProfileTaskContext ctx, ProductSpecificationAttribute psa)
		{
			if (psa == null)
				return null;

			var option = psa.SpecificationAttributeOption;

			dynamic expando = new ExpandoObject();
			expando._Entity = psa;

			expando.Id = psa.Id;
			expando.ProductId = psa.ProductId;
			expando.SpecificationAttributeOptionId = psa.SpecificationAttributeOptionId;
			expando.AllowFiltering = psa.AllowFiltering;
			expando.ShowOnProductPage = psa.ShowOnProductPage;
			expando.DisplayOrder = psa.DisplayOrder;

			dynamic expAttribute = new ExpandoObject();
			expAttribute._Entity = option.SpecificationAttribute;
			expAttribute.Id = option.SpecificationAttribute.Id;
			expAttribute.Name = option.SpecificationAttribute.GetLocalized(x => x.Name, ctx.Projection.LanguageId ?? 0, true, false);
			expAttribute.DisplayOrder = option.SpecificationAttribute.DisplayOrder;
			expAttribute._Localized = GetLocalized(ctx, option.SpecificationAttribute, x => x.Name);

			dynamic expOption = new ExpandoObject();
			expOption._Entity = option;
			expOption.Id = option.Id;
			expOption.SpecificationAttributeId = option.SpecificationAttributeId;
			expOption.Name = option.GetLocalized(x => x.Name, ctx.Projection.LanguageId ?? 0, true, false);
			expOption.DisplayOrder = option.DisplayOrder;
			expOption._Localized = GetLocalized(ctx, option, x => x.Name);
			expOption.SpecificationAttribute = expAttribute;

			expando.SpecificationAttributeOption = expOption;

			return expando;
		}

		private dynamic ToExpando(ExportProfileTaskContext ctx, GenericAttribute genericAttribute)
		{
			if (genericAttribute == null)
				return null;

			dynamic expando = new ExpandoObject();
			expando._Entity = genericAttribute;

			expando.Id = genericAttribute.Id;
			expando.EntityId = genericAttribute.EntityId;
			expando.KeyGroup = genericAttribute.KeyGroup;
			expando.Key = genericAttribute.Key;
			expando.Value = genericAttribute.Value;
			expando.StoreId = genericAttribute.StoreId;

			return expando;
		}

		private dynamic ToExpando(ExportProfileTaskContext ctx, NewsLetterSubscription subscription)
		{
			if (subscription == null)
				return null;

			dynamic expando = new ExpandoObject();
			expando._Entity = subscription;

			expando.Id = subscription.Id;
			expando.NewsLetterSubscriptionGuid = subscription.NewsLetterSubscriptionGuid;
			expando.Email = subscription.Email;
			expando.Active = subscription.Active;
			expando.CreatedOnUtc = subscription.CreatedOnUtc;
			expando.StoreId = subscription.StoreId;

			return expando;
		}

		private List<dynamic> ConvertToExpando(ExportProfileTaskContext ctx, Product product)
		{
			var result = new List<dynamic>();

			var languageId = (ctx.Projection.LanguageId ?? 0);
			var productTemplate = ctx.ProductTemplates.FirstOrDefault(x => x.Key == product.ProductTemplateId);
			var pictureSize = _mediaSettings.Value.ProductDetailsPictureSize;

			if (ctx.Supporting[ExportSupport.ProjectionMainPictureUrl] && ctx.Projection.PictureSize > 0)
				pictureSize = ctx.Projection.PictureSize;

			var perfLoadId = (ctx.IsPreview ? 0 : product.Id);	// perf preview (it's a compromise)
			var productPictures = ctx.DataContextProduct.ProductPictures.Load(perfLoadId);
			var productManufacturers = ctx.DataContextProduct.ProductManufacturers.Load(perfLoadId);
			var productCategories = ctx.DataContextProduct.ProductCategories.Load(product.Id);
			var productAttributes = ctx.DataContextProduct.Attributes.Load(product.Id);
			var productAttributeCombinations = ctx.DataContextProduct.AttributeCombinations.Load(product.Id);
			var productTags = ctx.DataContextProduct.ProductTags.Load(perfLoadId);
			var specificationAttributes = ctx.DataContextProduct.ProductSpecificationAttributes.Load(perfLoadId);

			dynamic expando = ToExpando(ctx, product);

			#region gerneral data

			expando._ProductTemplateViewPath = (productTemplate.Value == null ? "" : productTemplate.Value.ViewPath);

			expando._DetailUrl = ctx.Store.Url.EnsureEndsWith("/") + (string)expando.SeName;

			expando._CategoryName = null;

			if (ctx.Categories.Count > 0 && ctx.CategoryPathes.Count > 0)
			{
				expando._CategoryPath = _categoryService.Value.GetCategoryPath(
					product,
					null,
					x => ctx.CategoryPathes.ContainsKey(x) ? ctx.CategoryPathes[x] : null,
					(id, value) => ctx.CategoryPathes[id] = value,
					x => ctx.Categories.ContainsKey(x) ? ctx.Categories[x] : _categoryService.Value.GetCategoryById(x),
					productCategories.OrderBy(x => x.DisplayOrder).FirstOrDefault()
				);
			}
			else
			{
				expando._CategoryPath = null;
			}

			expando.ProductPictures = productPictures
				.OrderBy(x => x.DisplayOrder)
				.Select(x =>
				{
					dynamic exp = new ExpandoObject();
					exp._Entity = x;
					exp.Id = x.Id;
					exp.ProductId = x.ProductId;
					exp.DisplayOrder = x.DisplayOrder;
					exp.PictureId = x.PictureId;
					exp.Picture = ToExpando(ctx, x.Picture, _mediaSettings.Value.ProductThumbPictureSize, pictureSize);

					return exp;
				})
				.ToList();

			expando.ProductManufacturers = productManufacturers
				.OrderBy(x => x.DisplayOrder)
				.Select(x =>
				{
					dynamic exp = new ExpandoObject();
					exp._Entity = x;
					exp.Id = x.Id;
					exp.ProductId = x.ProductId;
					exp.DisplayOrder = x.DisplayOrder;
					exp.IsFeaturedProduct = x.IsFeaturedProduct;
					exp.ManufacturerId = x.ManufacturerId;
					exp.Manufacturer = ToExpando(ctx, x.Manufacturer);

					if (x.Manufacturer != null && x.Manufacturer.PictureId.HasValue)
						exp.Manufacturer.Picture = ToExpando(ctx, x.Manufacturer.Picture, _mediaSettings.Value.ManufacturerThumbPictureSize, _mediaSettings.Value.ManufacturerThumbPictureSize);
					else
						exp.Manufacturer.Picture = null;

					return exp;
				})
				.ToList();

			expando.ProductCategories = productCategories
				.OrderBy(x => x.DisplayOrder)
				.Select(x =>
				{
					dynamic exp = new ExpandoObject();
					exp._Entity = x;
					exp.Id = x.Id;
					exp.ProductId = x.ProductId;
					exp.DisplayOrder = x.DisplayOrder;
					exp.IsFeaturedProduct = x.IsFeaturedProduct;
					exp.CategoryId = x.CategoryId;
					exp.Category = ToExpando(ctx, x.Category);

					if (x.Category != null && x.Category.PictureId.HasValue)
						exp.Category.Picture = ToExpando(ctx, x.Category.Picture, _mediaSettings.Value.CategoryThumbPictureSize, _mediaSettings.Value.CategoryThumbPictureSize);

					if (expando._CategoryName == null)
						expando._CategoryName = (string)exp.Category.Name;

					return exp;
				})
				.ToList();

			expando.ProductAttributes = productAttributes
				.OrderBy(x => x.DisplayOrder)
				.Select(x => ToExpando(ctx, x))
				.ToList();

			expando.ProductAttributeCombinations = productAttributeCombinations
				.Select(x =>
				{
					dynamic exp = ToExpando(ctx, x);
					var assignedPictures = new List<dynamic>();

					foreach (int pictureId in x.GetAssignedPictureIds())
					{
						var assignedPicture = productPictures.FirstOrDefault(y => y.PictureId == pictureId);
						if (assignedPicture != null && assignedPicture.Picture != null)
						{
							assignedPictures.Add(ToExpando(ctx, assignedPicture.Picture, _mediaSettings.Value.ProductThumbPictureSize, pictureSize));
						}
					}

					exp.Pictures = assignedPictures;

					return exp;
				})
				.ToList();

			if (product.HasTierPrices)
			{
				var tierPrices = ctx.DataContextProduct.TierPrices.Load(product.Id)
					.RemoveDuplicatedQuantities();

				expando.TierPrices = tierPrices
					.Select(x =>
					{
						dynamic exp = new ExpandoObject();
						exp._Entity = x;
						exp.Id = x.Id;
						exp.ProductId = x.ProductId;
						exp.StoreId = x.StoreId;
						exp.CustomerRoleId = x.CustomerRoleId;
						exp.Quantity = x.Quantity;
						exp.Price = x.Price;
						return exp;
					})
					.ToList();
			}

			if (product.HasDiscountsApplied)
			{
				var appliedDiscounts = ctx.DataContextProduct.AppliedDiscounts.Load(product.Id);

				expando.AppliedDiscounts = appliedDiscounts
					.Select(x => ToExpando(ctx, x))
					.ToList();
			}

			expando.ProductTags = productTags
				.Select(x =>
				{
					dynamic exp = new ExpandoObject();
					exp._Entity = x;
					exp.Id = x.Id;
					exp.Name = x.GetLocalized(y => y.Name, languageId, true, false);
					exp.SeName = x.GetSeName(languageId);
					exp._Localized = GetLocalized(ctx, x, y => y.Name);
					return exp;
				})
				.ToList();

			expando.ProductSpecificationAttributes = specificationAttributes
				.Select(x => ToExpando(ctx, x))
				.ToList();

			if (product.ProductType == ProductType.BundledProduct)
			{
				var bundleItems = ctx.DataContextProduct.ProductBundleItems.Load(perfLoadId);

				expando.ProductBundleItems = bundleItems
					.Select(x =>
					{
						dynamic exp = new ExpandoObject();
						exp._Entity = x;
						exp.Id = x.Id;
						exp.ProductId = x.ProductId;
						exp.BundleProductId = x.BundleProductId;
						exp.Quantity = x.Quantity;
						exp.Discount = x.Discount;
						exp.DiscountPercentage = x.DiscountPercentage;
						exp.Name = x.GetLocalized(y => y.Name, languageId, true, false);
						exp.ShortDescription = x.GetLocalized(y => y.ShortDescription, languageId, true, false);
						exp.FilterAttributes = x.FilterAttributes;
						exp.HideThumbnail = x.HideThumbnail;
						exp.Visible = x.Visible;
						exp.Published = x.Published;
						exp.DisplayOrder = x.DisplayOrder;
						exp.CreatedOnUtc = x.CreatedOnUtc;
						exp.UpdatedOnUtc = x.UpdatedOnUtc;
						exp._Localized = GetLocalized(ctx, x, y => y.Name, y => y.ShortDescription);
						return exp;
					})
					.ToList();
			}

			#endregion

			#region more attribute controlled data

			if (ctx.Supports(ExportSupport.ProjectionDescription))
			{
				PrepareProductDescription(ctx, expando, product);
			}

			if (ctx.Supports(ExportSupport.ProjectionBrand))
			{
				string brand = null;
				var productManus = ctx.DataContextProduct.ProductManufacturers.Load(perfLoadId);

				if (productManus != null && productManus.Any())
					brand = productManus.First().Manufacturer.GetLocalized(x => x.Name, languageId, true, false);

				if (brand.IsEmpty())
					brand = ctx.Projection.Brand;

				expando._Brand = brand;
			}

			if (ctx.Supports(ExportSupport.ProjectionMainPictureUrl))
			{
				if (productPictures != null && productPictures.Any())
					expando._MainPictureUrl = _pictureService.Value.GetPictureUrl(productPictures.First().Picture, ctx.Projection.PictureSize, storeLocation: ctx.Store.Url);
				else
					expando._MainPictureUrl = _pictureService.Value.GetDefaultPictureUrl(ctx.Projection.PictureSize, storeLocation: ctx.Store.Url);
			}

			#endregion

			#region matter of data merging

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
					var values = _productAttributeParser.Value.ParseProductVariantAttributeValues(combination.AttributesXml, productAttributes, languageId);
					exp.Name = ((string)exp.Name).Grow(string.Join(", ", values), " ");
				}

				exp._BasePriceInfo = product.GetBasePriceInfo(_services.Localization, _priceFormatter.Value, _currencyService.Value, _taxService.Value,
					_priceCalculationService.Value, ctx.ContextCurrency, decimal.Zero, true);

				// navigation properties
				GetDeliveryTimeAndQuantityUnit(ctx, exp, product.DeliveryTimeId, product.QuantityUnitId);

				if (ctx.Supports(ExportSupport.ProjectionUseOwnProductNo) && product.ManufacturerPartNumber.IsEmpty())
				{
					exp.ManufacturerPartNumber = product.Sku;
				}

				if (ctx.Supports(ExportSupport.ProjectionShippingTime))
				{
					dynamic deliveryTime = exp.DeliveryTime;
					exp._ShippingTime = (deliveryTime == null ? ctx.Projection.ShippingTime : deliveryTime.Name);
				}

				if (ctx.Supports(ExportSupport.ProjectionShippingCosts))
				{
					exp._FreeShippingThreshold = ctx.Projection.FreeShippingThreshold;

					if (product.IsFreeShipping || (ctx.Projection.FreeShippingThreshold.HasValue && (decimal)exp.Price >= ctx.Projection.FreeShippingThreshold.Value))
						exp._ShippingCosts = decimal.Zero;
					else
						exp._ShippingCosts = ctx.Projection.ShippingCosts;
				}

				if (ctx.Supports(ExportSupport.ProjectionOldPrice))
				{
					if (product.OldPrice != decimal.Zero && product.OldPrice != (decimal)exp.Price && !(product.ProductType == ProductType.BundledProduct && product.BundlePerItemPricing))
					{
						if (ctx.Projection.ConvertNetToGrossPrices)
						{
							decimal taxRate;
							exp._OldPrice = _taxService.Value.GetProductPrice(product, product.OldPrice, true, ctx.ContextCustomer, out taxRate);
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

				if (ctx.Supports(ExportSupport.ProjectionSpecialPrice))
				{
					exp._SpecialPrice = null;
					exp._RegularPrice = null;	// price if a special price would not exist

					if (!(product.ProductType == ProductType.BundledProduct && product.BundlePerItemPricing))
					{
						var specialPrice = _priceCalculationService.Value.GetSpecialPrice(product);

						exp._SpecialPrice = ConvertPrice(ctx, product, specialPrice);

						if (specialPrice.HasValue)
						{
							decimal tmpSpecialPrice = product.SpecialPrice.Value;
							product.SpecialPrice = null;
							exp._RegularPrice = CalculatePrice(ctx, product, combination != null);
							product.SpecialPrice = tmpSpecialPrice;
						}
					}
				}
			};

			#endregion

			if (!ctx.IsPreview && ctx.Projection.AttributeCombinationAsProduct && productAttributeCombinations.Where(x => x.IsActive).Count() > 0)
			{
				// EF does not support entities to be constructed in a LINQ to entities query.
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

		private List<dynamic> ConvertToExpando(ExportProfileTaskContext ctx, Order order)
		{
			var result = new List<dynamic>();

			ctx.DataContextOrder.Addresses.Collect(order.ShippingAddressId ?? 0);

			var perfLoadId = (ctx.IsPreview ? 0 : order.Id);
			var addresses = ctx.DataContextOrder.Addresses.Load(ctx.IsPreview ? 0 : order.BillingAddressId);
			var customers = ctx.DataContextOrder.Customers.Load(order.CustomerId);
			var rewardPointsHistories = ctx.DataContextOrder.RewardPointsHistories.Load(ctx.IsPreview ? 0 : order.CustomerId);
			var orderItems = ctx.DataContextOrder.OrderItems.Load(perfLoadId);
			var shipments = ctx.DataContextOrder.Shipments.Load(perfLoadId);

			dynamic expando = ToExpando(ctx, order);

			if (ctx.Stores.ContainsKey(order.StoreId))
			{
				expando.Store = ToExpando(ctx, ctx.Stores[order.StoreId]);
			}

			expando.Customer = ToExpando(ctx, customers.FirstOrDefault(x => x.Id == order.CustomerId));

			expando.Customer.RewardPointsHistory = rewardPointsHistories
				.Select(x => ToExpando(ctx, x))
				.ToList();

			if (rewardPointsHistories.Count > 0)
			{
				expando.Customer._RewardPointsBalance = rewardPointsHistories
					.OrderByDescending(x => x.CreatedOnUtc)
					.ThenByDescending(x => x.Id)
					.FirstOrDefault()
					.PointsBalance;
			}

			expando.BillingAddress = ToExpando(ctx, addresses.FirstOrDefault(x => x.Id == order.BillingAddressId));

			if (order.ShippingAddressId.HasValue)
			{
				expando.ShippingAddress = ToExpando(ctx, addresses.FirstOrDefault(x => x.Id == order.ShippingAddressId.Value));
			}

			expando.OrderItems = orderItems
				.Select(e =>
				{
					dynamic exp = ToExpando(ctx, e);
					var productTemplate = ctx.ProductTemplates.FirstOrDefault(x => x.Key == e.Product.ProductTemplateId);

					exp.Product._ProductTemplateViewPath = (productTemplate.Value == null ? "" : productTemplate.Value.ViewPath);

					exp.Product._BasePriceInfo = e.Product.GetBasePriceInfo(_services.Localization, _priceFormatter.Value, _currencyService.Value, _taxService.Value,
						_priceCalculationService.Value, ctx.ContextCurrency, decimal.Zero, true);

					GetDeliveryTimeAndQuantityUnit(ctx, exp.Product, e.Product.DeliveryTimeId, e.Product.QuantityUnitId);

					return exp;
				})
				.ToList();

			expando.Shipments = shipments
				.Select(x => ToExpando(ctx, x))
				.ToList();

			result.Add(expando);
			return result;
		}

		private List<dynamic> ConvertToExpando(ExportProfileTaskContext ctx, Manufacturer manu)
		{
			var result = new List<dynamic>();

			var productManufacturers = ctx.DataContextManufacturer.ProductManufacturers.Load(manu.Id);

			dynamic expando = ToExpando(ctx, manu);

			if (!ctx.IsPreview && manu.PictureId.HasValue)
			{
				var pictures = ctx.DataContextManufacturer.Pictures.Load(manu.PictureId.Value);

				if (pictures.Count > 0)
					expando.Picture = ToExpando(ctx, pictures.First(), _mediaSettings.Value.ManufacturerThumbPictureSize, _mediaSettings.Value.ManufacturerThumbPictureSize);
			}

			expando.ProductManufacturers = productManufacturers
				.OrderBy(x => x.DisplayOrder)
				.Select(x =>
				{
					dynamic exp = new ExpandoObject();
					exp._Entity = x;
					exp.Id = x.Id;
					exp.ProductId = x.ProductId;
					exp.DisplayOrder = x.DisplayOrder;
					exp.IsFeaturedProduct = x.IsFeaturedProduct;
					exp.ManufacturerId = x.ManufacturerId;

					return exp;
				})
				.ToList();

			result.Add(expando);
			return result;
		}

		private List<dynamic> ConvertToExpando(ExportProfileTaskContext ctx, Category category)
		{
			var result = new List<dynamic>();

			var productCategories = ctx.DataContextCategory.ProductCategories.Load(category.Id);

			dynamic expando = ToExpando(ctx, category);

			if (!ctx.IsPreview && category.PictureId.HasValue)
			{
				var pictures = ctx.DataContextCategory.Pictures.Load(category.PictureId.Value);

				if (pictures.Count > 0)
					expando.Picture = ToExpando(ctx, pictures.First(), _mediaSettings.Value.CategoryThumbPictureSize, _mediaSettings.Value.CategoryThumbPictureSize);
			}

			expando.ProductCategories = productCategories
				.OrderBy(x => x.DisplayOrder)
				.Select(x =>
				{
					dynamic exp = new ExpandoObject();
					exp._Entity = x;
					exp.Id = x.Id;
					exp.ProductId = x.ProductId;
					exp.DisplayOrder = x.DisplayOrder;
					exp.IsFeaturedProduct = x.IsFeaturedProduct;
					exp.CategoryId = x.CategoryId;

					return exp;
				})
				.ToList();

			result.Add(expando);
			return result;
		}

		private List<dynamic> ConvertToExpando(ExportProfileTaskContext ctx, Customer customer)
		{
			var result = new List<dynamic>();

			var perfLoadId = (ctx.IsPreview ? 0 : customer.Id);
			var genericAttributes = ctx.DataContextCustomer.GenericAttributes.Load(perfLoadId);

			dynamic expando = ToExpando(ctx, customer);

			expando.BillingAddress = ToExpando(ctx, customer.BillingAddress);
			expando.ShippingAddress = ToExpando(ctx, customer.ShippingAddress);

			expando.Addresses = customer.Addresses
				.Select(x => ToExpando(ctx, x))
				.ToList();

			expando.CustomerRoles = customer.CustomerRoles
				.Select(x =>
				{
					dynamic exp = new ExpandoObject();
					exp._Entity = x;
					exp.Id = x.Id;
					exp.Name = x.Name;
					exp.FreeShipping = x.FreeShipping;
					exp.TaxExempt = x.TaxExempt;
					exp.TaxDisplayType = x.TaxDisplayType;
					exp.Active = x.Active;
					exp.IsSystemRole = x.IsSystemRole;
					exp.SystemName = x.SystemName;

					return exp;
				})
				.ToList();

			expando._GenericAttributes = genericAttributes
				.Select(x => ToExpando(ctx, x))
				.ToList();

			expando._HasNewsletterSubscription = ctx.NewsletterSubscriptions.Contains(customer.Email, StringComparer.CurrentCultureIgnoreCase);

			result.Add(expando);
			return result;
		}

		private List<dynamic> ConvertToExpando(ExportProfileTaskContext ctx, NewsLetterSubscription subscription)
		{
			var result = new List<dynamic>();

			dynamic expando = ToExpando(ctx, subscription);

			result.Add(expando);
			return result;
		}

		#endregion

		#region Getting data

		private IQueryable<Product> GetProductQuery(ExportProfileTaskContext ctx, int skip, int take)
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
					searchContext.CreatedFromUtc = _dateTimeHelper.Value.ConvertToUtcTime(ctx.Filter.CreatedFrom.Value, _dateTimeHelper.Value.CurrentTimeZone);

				if (ctx.Filter.CreatedTo.HasValue)
					searchContext.CreatedToUtc = _dateTimeHelper.Value.ConvertToUtcTime(ctx.Filter.CreatedTo.Value, _dateTimeHelper.Value.CurrentTimeZone);

				query = _productService.Value.PrepareProductSearchQuery(searchContext);

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

		private List<Product> GetProducts(ExportProfileTaskContext ctx, int skip)
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

						foreach (var associatedProduct in _productService.Value.SearchProducts(associatedSearchContext))
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

				_services.DbContext.DetachEntities<Product>(result);
			}
			catch { }

			return result;
		}

		private IQueryable<Order> GetOrderQuery(ExportProfileTaskContext ctx, int skip, int take)
		{
			var query = _orderService.Value.GetOrders(
				ctx.Profile.PerStore ? ctx.Store.Id : ctx.Filter.StoreId,
				ctx.Projection.CustomerId ?? 0,
				ctx.Filter.CreatedFrom.HasValue ? (DateTime?)_dateTimeHelper.Value.ConvertToUtcTime(ctx.Filter.CreatedFrom.Value, _dateTimeHelper.Value.CurrentTimeZone) : null,
				ctx.Filter.CreatedTo.HasValue ? (DateTime?)_dateTimeHelper.Value.ConvertToUtcTime(ctx.Filter.CreatedTo.Value, _dateTimeHelper.Value.CurrentTimeZone) : null,
				ctx.Filter.OrderStatusIds,
				ctx.Filter.PaymentStatusIds,
				ctx.Filter.ShippingStatusIds,
				null,
				null,
				null);

			if (ctx.EntityIdsSelected.Count > 0)
				query = query.Where(x => ctx.EntityIdsSelected.Contains(x.Id));

			query = query.OrderByDescending(x => x.CreatedOnUtc);

			if (skip > 0)
				query = query.Skip(skip);

			if (take != int.MaxValue)
				query = query.Take(take);

			return query;
		}

		private List<Order> GetOrders(ExportProfileTaskContext ctx, int skip)
		{
			var orders = GetOrderQuery(ctx, skip, PageSize).ToList();

			if (ctx.Projection.OrderStatusChange != ExportOrderStatusChange.None)
			{
				ctx.EntityIdsLoaded = ctx.EntityIdsLoaded
					.Union(orders.Select(x => x.Id))
					.Distinct()
					.ToList();
			}

			try
			{
				SetProgress(ctx, orders.Count);

				_services.DbContext.DetachEntities<Order>(orders);
			}
			catch { }

			return orders;
		}

		private IQueryable<Manufacturer> GetManufacturerQuery(ExportProfileTaskContext ctx, int skip, int take)
		{
			var showHidden = !ctx.Filter.IsPublished.HasValue;
			var storeId = (ctx.Profile.PerStore ? ctx.Store.Id : ctx.Filter.StoreId);

			var query = _manufacturerService.Value.GetManufacturers(showHidden, storeId);

			query = query.OrderBy(x => x.DisplayOrder);

			if (skip > 0)
				query = query.Skip(skip);

			if (take != int.MaxValue)
				query = query.Take(take);

			return query;
		}

		private List<Manufacturer> GetManufacturers(ExportProfileTaskContext ctx, int skip)
		{
			var manus = GetManufacturerQuery(ctx, skip, PageSize).ToList();

			try
			{
				SetProgress(ctx, manus.Count);

				_services.DbContext.DetachEntities<Manufacturer>(manus);
			}
			catch { }

			return manus;
		}

		private IQueryable<Category> GetCategoryQuery(ExportProfileTaskContext ctx, int skip, int take)
		{
			var showHidden = !ctx.Filter.IsPublished.HasValue;
			var storeId = (ctx.Profile.PerStore ? ctx.Store.Id : ctx.Filter.StoreId);

			var query = _categoryService.Value.GetCategories(null, showHidden, null, true, storeId);

			query = query
				.OrderBy(x => x.ParentCategoryId)
				.ThenBy(x => x.DisplayOrder);

			if (skip > 0)
				query = query.Skip(skip);

			if (take != int.MaxValue)
				query = query.Take(take);

			return query;
		}

		private List<Category> GetCategories(ExportProfileTaskContext ctx, int skip)
		{
			var categories = GetCategoryQuery(ctx, skip, PageSize).ToList();

			try
			{
				SetProgress(ctx, categories.Count);

				_services.DbContext.DetachEntities<Category>(categories);
			}
			catch { }

			return categories;
		}

		private IQueryable<Customer> GetCustomerQuery(ExportProfileTaskContext ctx, int skip, int take)
		{
			var query = _customerRepository.Value.TableUntracked
				.Expand(x => x.BillingAddress)
				.Expand(x => x.ShippingAddress)
				.Expand(x => x.Addresses.Select(y => y.Country))
				.Expand(x => x.Addresses.Select(y => y.StateProvince))
				.Expand(x => x.CustomerRoles)
				.Where(x => !x.Deleted);

			if (ctx.EntityIdsSelected.Count > 0)
			{
				query = query.Where(x => ctx.EntityIdsSelected.Contains(x.Id));
			}

			query = query.OrderByDescending(x => x.CreatedOnUtc);

			if (skip > 0)
				query = query.Skip(skip);

			if (take != int.MaxValue)
				query = query.Take(take);

			return query;
		}

		private List<Customer> GetCustomers(ExportProfileTaskContext ctx, int skip)
		{
			var customers = GetCustomerQuery(ctx, skip, PageSize).ToList();

			try
			{
				SetProgress(ctx, customers.Count);

				_services.DbContext.DetachEntities<Customer>(customers);
			}
			catch { }

			return customers;
		}

		private IQueryable<NewsLetterSubscription> GetNewsLetterSubscriptionQuery(ExportProfileTaskContext ctx, int skip, int take)
		{
			var storeId = (ctx.Profile.PerStore ? ctx.Store.Id : ctx.Filter.StoreId);

			var query = _subscriptionRepository.Value.TableUntracked;

			if (storeId > 0)
			{
				query = query.Where(x => x.StoreId == storeId);
			}

			query = query
				.OrderBy(x => x.StoreId)
				.ThenBy(x => x.Email);

			if (skip > 0)
				query = query.Skip(skip);

			if (take != int.MaxValue)
				query = query.Take(take);

			return query;
		}

		private List<NewsLetterSubscription> GetNewsLetterSubscriptions(ExportProfileTaskContext ctx, int skip)
		{
			var subscriptions = GetNewsLetterSubscriptionQuery(ctx, skip, PageSize).ToList();

			try
			{
				SetProgress(ctx, subscriptions.Count);

				_services.DbContext.DetachEntities<NewsLetterSubscription>(subscriptions);
			}
			catch { }

			return subscriptions;
		}

		#endregion

		private List<Store> Init(ExportProfileTaskContext ctx, int? totalRecords = null)
		{
			// Init base things that are even required for preview. Init all other things (regular export) in ExportCoreOuter.
			List<Store> result = null;

			if (ctx.Projection.CurrencyId.HasValue)
				ctx.ContextCurrency = _currencyService.Value.GetCurrencyById(ctx.Projection.CurrencyId.Value);
			else
				ctx.ContextCurrency = _services.WorkContext.WorkingCurrency;

			if (ctx.Projection.CustomerId.HasValue)
				ctx.ContextCustomer = _customerService.Value.GetCustomerById(ctx.Projection.CustomerId.Value);
			else
				ctx.ContextCustomer = _services.WorkContext.CurrentCustomer;

			if (ctx.Projection.LanguageId.HasValue)
				ctx.ContextLanguage = _languageService.Value.GetLanguageById(ctx.Projection.LanguageId.Value);
			else
				ctx.ContextLanguage = _services.WorkContext.WorkingLanguage;

			ctx.Stores = _services.StoreService.GetAllStores().ToDictionary(x => x.Id, x => x);
			ctx.Languages = _languageService.Value.GetAllLanguages(true).ToDictionary(x => x.Id, x => x);

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
				ctx.Store = store;

				int totalCount = 0;

				if (totalRecords.HasValue)
				{
					totalCount = totalRecords.Value;	// speed up preview by not counting total at each page
				}
				else
				{
					switch (ctx.Provider.Value.EntityType)
					{
						case ExportEntityType.Product:
							totalCount = GetProductQuery(ctx, ctx.Profile.Offset, int.MaxValue).Count();
							break;
						case ExportEntityType.Order:
							totalCount = GetOrderQuery(ctx, ctx.Profile.Offset, int.MaxValue).Count();
							break;
						case ExportEntityType.Manufacturer:
							totalCount = GetManufacturerQuery(ctx, ctx.Profile.Offset, int.MaxValue).Count();
							break;
						case ExportEntityType.Category:
							totalCount = GetCategoryQuery(ctx, ctx.Profile.Offset, int.MaxValue).Count();
							break;
						case ExportEntityType.Customer:
							totalCount = GetCustomerQuery(ctx, ctx.Profile.Offset, int.MaxValue).Count();
							break;
						case ExportEntityType.NewsLetterSubscription:
							totalCount = GetNewsLetterSubscriptionQuery(ctx, ctx.Profile.Offset, int.MaxValue).Count();
							break;
					}
				}

				ctx.RecordsPerStore.Add(store.Id, totalCount);
			}

			return result;
		}

		private void ExportCoreInner(ExportProfileTaskContext ctx, Store store)
		{
			if (ctx.Export.Abort != ExportAbortion.None)
				return;

			int fileIndex = 0;

			ctx.Store = store;

			{
				var logHead = new StringBuilder();
				logHead.AppendLine();
				logHead.AppendLine(new string('-', 40));
				logHead.AppendLine("SmartStore.NET:\t\tv." + SmartStoreVersion.CurrentFullVersion);
				logHead.Append("Export profile:\t\t" + ctx.Profile.Name);
				logHead.AppendLine(ctx.Profile.Id == 0 ? " (volatile)" : " (Id {0})".FormatInvariant(ctx.Profile.Id));

				logHead.AppendLine("Export provider:\t{0} ({1})".FormatInvariant(ctx.Provider.Metadata.FriendlyName, ctx.Profile.ProviderSystemName));

				var plugin = ctx.Provider.Metadata.PluginDescriptor;
				logHead.Append("Plugin:\t\t\t\t");
				logHead.AppendLine(plugin == null ? "".NaIfEmpty() : "{0} ({1}) v.{2}".FormatInvariant(plugin.FriendlyName, plugin.SystemName, plugin.Version.ToString()));

				logHead.AppendLine("Entity:\t\t\t\t" + ctx.Provider.Value.EntityType.ToString());

				var storeInfo = (ctx.Profile.PerStore ? "{0} (Id {1})".FormatInvariant(ctx.Store.Name, ctx.Store.Id) : "All stores");
				logHead.Append("Store:\t\t\t\t" + storeInfo);

				ctx.Log.Information(logHead.ToString());
			}

			ctx.Export.Store = ToExpando(ctx, ctx.Store);

			ctx.Export.MaxFileNameLength = _dataExchangeSettings.Value.MaxFileNameLength;

			ctx.Export.FileExtension = (ctx.Provider.Value.FileExtension.HasValue() ? ctx.Provider.Value.FileExtension.ToLower().EnsureStartsWith(".") : "");

			ctx.Export.HasPublicDeployment = ctx.Profile.Deployments.Any(x => x.IsPublic && x.DeploymentType == ExportDeploymentType.FileSystem);

			ctx.Export.PublicFolderPath = (ctx.Export.HasPublicDeployment ? Path.Combine(HttpRuntime.AppDomainAppPath, PublicFolder) : null);


			using (var segmenter = CreateSegmenter(ctx))
			{
				if (segmenter == null)
				{
					throw new SmartException("Unsupported entity type '{0}'".FormatInvariant(ctx.Provider.Value.EntityType.ToString()));
				}

				if (segmenter.RecordTotal <= 0)
				{
					ctx.Log.Information("There are no records to export");
				}

				while (ctx.Export.Abort == ExportAbortion.None && segmenter.HasData)
				{
					segmenter.RecordPerSegmentCount = 0;
					ctx.Export.RecordsSucceeded = 0;

					try
					{
						if (ctx.IsFileBasedExport)
						{
							var resolvedPattern = ctx.Profile.ResolveFileNamePattern(ctx.Store, ++fileIndex, ctx.Export.MaxFileNameLength);

							ctx.Export.FileName = resolvedPattern + ctx.Export.FileExtension;
							ctx.Export.FilePath = Path.Combine(ctx.Export.Folder, ctx.Export.FileName);

							if (ctx.Export.HasPublicDeployment)
								ctx.Export.PublicFileUrl = ctx.Store.Url.EnsureEndsWith("/") + PublicFolder.EnsureEndsWith("/") + ctx.Export.FileName;

							ctx.Provider.Value.Execute(ctx.Export);

							// create info for deployment list in profile edit
							if (File.Exists(ctx.Export.FilePath))
							{
								ctx.Result.Files.Add(new ExportExecuteResult.ExportFileInfo
								{
									StoreId = ctx.Store.Id,
									FileName = ctx.Export.FileName
								});
							}
						}
						else
						{
							ctx.Provider.Value.Execute(ctx.Export);
						}

						ctx.Log.Information("Provider reports {0} successful exported record(s)".FormatInvariant(ctx.Export.RecordsSucceeded));
					}
					catch (Exception exc)
					{
						ctx.Export.Abort = ExportAbortion.Hard;
						ctx.Log.Error("The provider failed to execute the export: " + exc.ToAllMessages(), exc);
						ctx.Result.LastError = exc.ToString();
					}

					if (ctx.Export.IsMaxFailures)
						ctx.Log.Warning("Export aborted. The maximum number of failures has been reached");

					if (ctx.TaskContext.CancellationToken.IsCancellationRequested)
						ctx.Log.Warning("Export aborted. A cancellation has been requested");
				}

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
			FileSystemHelper.ClearDirectory(ctx.FolderContent, false);
			FileSystemHelper.Delete(ctx.ZipPath);

			using (_rwLock.GetWriteLock())
			using (var logger = new TraceLogger(ctx.LogPath))
			{
				try
				{
					if (!ctx.Provider.IsValid())
					{
						throw new SmartException("Export aborted because the export provider is not valid");
					}

					ctx.Log = logger;
					ctx.Export.Log = logger;
					ctx.ProgressInfo = _services.Localization.GetResource("Admin.DataExchange.Export.ProgressInfo");

					if (ctx.Profile.ProviderConfigData.HasValue())
					{
						var configInfo = ctx.Provider.Value.ConfigurationInfo;
						if (configInfo != null)
						{
							ctx.Export.ConfigurationData = XmlHelper.Deserialize(ctx.Profile.ProviderConfigData, configInfo.ModelType);
						}
					}

					using (var scope = new DbContextScope(_services.DbContext, autoDetectChanges: false, proxyCreation: true, validateOnSave: false, forceNoTracking: true))
					{
						ctx.DeliveryTimes = _deliveryTimeService.Value.GetAllDeliveryTimes().ToDictionary(x => x.Id, x => x);
						ctx.QuantityUnits = _quantityUnitService.Value.GetAllQuantityUnits().ToDictionary(x => x.Id, x => x);
						ctx.ProductTemplates = _productTemplateService.Value.GetAllProductTemplates().ToDictionary(x => x.Id, x => x);

						if (ctx.Provider.Value.EntityType == ExportEntityType.Product)
						{
							var allCategories = _categoryService.Value.GetAllCategories(showHidden: true, applyNavigationFilters: false);
							ctx.Categories = allCategories.ToDictionary(x => x.Id);
						}

						if (ctx.Provider.Value.EntityType == ExportEntityType.Order)
						{
							ctx.Countries = _countryService.Value.GetAllCountries(true).ToDictionary(x => x.Id, x => x);
						}

						if (ctx.Provider.Value.EntityType == ExportEntityType.Customer)
						{
							var subscriptionEmails = _subscriptionRepository.Value.TableUntracked
								.Where(x => x.Active)
								.Select(x => x.Email)
								.Distinct()
								.ToList();

							ctx.NewsletterSubscriptions = new HashSet<string>(subscriptionEmails, StringComparer.OrdinalIgnoreCase);
						}

						var stores = Init(ctx);

						ctx.Export.Language = ToExpando(ctx, ctx.ContextLanguage);
						ctx.Export.Customer = ToExpando(ctx, ctx.ContextCustomer);
						ctx.Export.Currency = ToExpando(ctx, ctx.ContextCurrency);

						stores.ForEach(x => ExportCoreInner(ctx, x));
					}

					if (!ctx.IsPreview && ctx.Export.Abort != ExportAbortion.Hard)
					{
						if (ctx.IsFileBasedExport)
						{
							if (ctx.Profile.CreateZipArchive || ctx.Profile.Deployments.Any(x => x.Enabled && x.CreateZip))
							{
								ZipFile.CreateFromDirectory(ctx.FolderContent, ctx.ZipPath, CompressionLevel.Fastest, true);
							}

							SetProgress(ctx, _services.Localization.GetResource("Common.Deployment"));

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
									logger.Error("Deployment \"{0}\" of type {1} failed: {2}".FormatInvariant(
										deployment.Name, deployment.DeploymentType.ToString(), exc.Message), exc);
								}
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
					ctx.Result.LastError = exc.ToString();
				}
				finally
				{
					try
					{
						if (!ctx.IsPreview && ctx.Profile.Id != 0)
						{
							ctx.Profile.ResultInfo = XmlHelper.Serialize<ExportExecuteResult>(ctx.Result);

							_exportService.Value.UpdateExportProfile(ctx.Profile);
						}
					}
					catch { }

					try
					{
						if (ctx.IsFileBasedExport && ctx.Export.Abort != ExportAbortion.Hard && ctx.Profile.Cleanup)
						{
							FileSystemHelper.ClearDirectory(ctx.FolderContent, false);
						}
					}
					catch { }

					try
					{
						ctx.NewsletterSubscriptions.Clear();
						ctx.ProductTemplates.Clear();
						ctx.Countries.Clear();
						ctx.Stores.Clear();
						ctx.QuantityUnits.Clear();
						ctx.DeliveryTimes.Clear();
						ctx.CategoryPathes.Clear();
						ctx.Categories.Clear();
						ctx.EntityIdsSelected.Clear();
						ctx.DataContextProduct = null;
						ctx.DataContextOrder = null;
						ctx.DataContextManufacturer = null;
						ctx.DataContextCategory = null;
						ctx.DataContextCustomer = null;

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
								var entities = _orderRepository.Value.Table.Where(x => chunk.Contains(x.Id)).ToList();

								entities.ForEach(x => x.OrderStatusId = (orderStatusId ?? x.OrderStatusId));

								_services.DbContext.SaveChanges();
							}
						}

						logger.Information("Updated order status for {0} order(s).".FormatInvariant(ctx.EntityIdsLoaded.Count()));
					}
					catch (Exception exc)
					{
						logger.Error(exc);
						ctx.Result.LastError = exc.ToString();
					}
				}
			}
		}


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

		/// <summary>
		/// Export using async runner and a volatile profile. Helper for internal exports to reduce duplicate code.
		/// </summary>
		/// <param name="providerSystemName">Provider system name</param>
		/// <param name="selectedEntityIds">Entity identifiers of entities to be exported. Can be <c>null</c> to export all entities.</param>
		/// <param name="error">Last error</param>
		/// <returns>File stream result of the first export data file</returns>
		public static FileStreamResult Export(string providerSystemName, string selectedEntityIds, string downloadFileName, out string error)
		{
			Guard.ArgumentNotEmpty(() => providerSystemName);

			error = null;
			var cancellation = new CancellationTokenSource(TimeSpan.FromHours(3.0));

			var task = AsyncRunner.Run<ExportExecuteResult>((container, ct) =>
			{
				var exportTask = new ExportProfileTask();
				return exportTask.Execute(providerSystemName, container, ct, null, selectedEntityIds);
			},
			cancellation.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);

			task.Wait();

			if (task.Result != null && task.Result.Succeeded && task.Result.FileFolder.HasValue() && task.Result.Files.Count > 0)
			{
				var fileName = task.Result.Files.First().FileName;
				var filePath = Path.Combine(task.Result.FileFolder, fileName);

				var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
				var result = new FileStreamResult(stream, MimeTypes.MapNameToMimeType(fileName));

				if (downloadFileName.HasValue())
					result.FileDownloadName = downloadFileName.ToValidFileName() + Path.GetExtension(filePath);
				else
					result.FileDownloadName = task.Result.DownloadFileName;

				return result;
			}

			if (task.Result != null)
				error = task.Result.LastError;

			return null;
		}

		/// <summary>
		/// Export by executing schedule task of export profile
		/// </summary>
		/// <param name="taskContext">Schedule task execution context</param>
		public void Execute(TaskExecutionContext taskContext)
		{
			InitDependencies(taskContext);

			var profileId = taskContext.ScheduleTask.Alias.ToInt();
			var profile = _exportService.Value.GetExportProfileById(profileId);

			var selectedIdsCacheKey = profile.GetSelectedEntityIdsCacheKey();
			var selectedEntityIds = HttpRuntime.Cache[selectedIdsCacheKey] as string;

			var provider = _exportService.Value.LoadProvider(profile.ProviderSystemName);

			if (provider == null)
				throw new SmartException(_services.Localization.GetResource("Admin.Common.ProviderNotLoaded").FormatInvariant(profile.ProviderSystemName.NaIfEmpty()));

			var ctx = new ExportProfileTaskContext(taskContext, profile, provider, selectedEntityIds);

			HttpRuntime.Cache.Remove(selectedIdsCacheKey);

			ExportCoreOuter(ctx);

			taskContext.CancellationToken.ThrowIfCancellationRequested();
		}

		/// <summary>
		/// Direct export using an export profile
		/// </summary>
		/// <param name="providerSystemName">Provider system name</param>
		/// <param name="context">Component context</param>
		/// <param name="profile">Export profile. Can be <c>null</c> to use a volatile profile.</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <param name="selectedEntityIds">Entity identifiers of entities to be exported. Can be <c>null</c> to export all entities.</param>
		/// <param name="customProperties">Any data passed on IExportExecuteContext.CustomProperties</param>
		/// <param name="queryProducts">Product query that supersede profile filtering</param>
		/// <returns>Export execute result</returns>
		public ExportExecuteResult Execute(string providerSystemName,
			IComponentContext context,
			CancellationToken cancellationToken,
			ExportProfile profile = null,
			string selectedEntityIds = null,
			Dictionary<string, object> customProperties = null,
			IQueryable<Product> queryProducts = null)
		{
			Guard.ArgumentNotEmpty(() => providerSystemName);
			Guard.ArgumentNotNull(() => context);

			var taskContext = new TaskExecutionContext(context, null);
			taskContext.CancellationToken = cancellationToken;

			InitDependencies(taskContext);

			var provider = _exportService.Value.LoadProvider(providerSystemName);

			if (provider == null)
				throw new SmartException(_services.Localization.GetResource("Admin.Common.ProviderNotLoaded").FormatInvariant(providerSystemName.NaIfEmpty()));

			if (profile == null)
				profile = _exportService.Value.CreateVolatileProfile(provider);

			var ctx = new ExportProfileTaskContext(taskContext, profile, provider, selectedEntityIds);
			ctx.QueryProducts = queryProducts;

			if (customProperties != null)
			{
				foreach (var item in customProperties)
					ctx.Export.CustomProperties.Add(item.Key, item.Value);
			}

			ExportCoreOuter(ctx);

			if (ctx.Result != null && ctx.Result.Succeeded && ctx.Result.Files.Count > 0)
			{
				string prefix = null;
				string suffix = null;
				var extension = Path.GetExtension(ctx.Result.Files.First().FileName);

				if (provider.Value.EntityType == ExportEntityType.Product)
					prefix = _services.Localization.GetResource("Admin.Catalog.Products");
				else if (provider.Value.EntityType == ExportEntityType.Order)
					prefix = _services.Localization.GetResource("Admin.Orders");
				else if (provider.Value.EntityType == ExportEntityType.Category)
					prefix = _services.Localization.GetResource("Admin.Catalog.Categories");
				else if (provider.Value.EntityType == ExportEntityType.Manufacturer)
					prefix = _services.Localization.GetResource("Admin.Catalog.Manufacturers");
				else if (provider.Value.EntityType == ExportEntityType.Customer)
					prefix = _services.Localization.GetResource("Admin.Customers");
				else if (provider.Value.EntityType == ExportEntityType.NewsLetterSubscription)
					prefix = _services.Localization.GetResource("Admin.Promotions.NewsLetterSubscriptions");
				else
					prefix = provider.Value.EntityType.ToString();

				if (selectedEntityIds.HasValue())
					suffix = (selectedEntityIds.Contains(",") ? _services.Localization.GetResource("Admin.Common.Selected") : selectedEntityIds);
				else
					suffix = _services.Localization.GetResource("Common.All");

				ctx.Result.DownloadFileName = string.Concat(prefix, "-", suffix).ToLower().ToValidFileName() + extension;
			}

			return ctx.Result;
		}

		/// <summary>
		/// Get preview data of an export profile
		/// </summary>
		/// <param name="profile">Export profile</param>
		/// <param name="context">Component context</param>
		/// <param name="pageIndex">Page index</param>
		/// <param name="totalRecords">Number of total records</param>
		/// <param name="previewData">Action to process preview data</param>
		public void Preview(ExportProfile profile, Provider<IExportProvider> provider, IComponentContext context, int pageIndex, int totalRecords, Action<dynamic> previewData)
		{
			Guard.ArgumentNotNull(() => profile);
			Guard.ArgumentNotNull(() => provider);
			Guard.ArgumentNotNull(() => context);

			var taskContext = new TaskExecutionContext(context, null);
			var cancellation = new CancellationTokenSource(TimeSpan.FromMinutes(5.0));

			taskContext.CancellationToken = cancellation.Token;

			InitDependencies(taskContext);

			var ctx = new ExportProfileTaskContext(taskContext, profile, provider, null, previewData);

			var unused = Init(ctx, totalRecords);

			using (var segmenter = CreateSegmenter(ctx, pageIndex))
			{
				if (segmenter == null)
				{
					throw new SmartException("Unsupported entity type '{0}'".FormatInvariant(ctx.Provider.Value.EntityType.ToString()));
				}

				while (segmenter.HasData)
				{
					segmenter.RecordPerSegmentCount = 0;

					while (segmenter.ReadNextSegment())
					{
						segmenter.CurrentSegment.ForEach(x => ctx.PreviewData(x));
					}
				}
			}

			if (ctx.Result.LastError.HasValue())
			{
				_services.Notifier.Error(ctx.Result.LastError);
			}
		}

		/// <summary>
		/// Get the number of total export records
		/// </summary>
		/// <param name="profile">Export profile</param>
		/// <param name="provider">Export provider</param>
		/// <param name="context">Component context</param>
		/// <returns>Number of total records</returns>
		public int GetRecordCount(ExportProfile profile, Provider<IExportProvider> provider, IComponentContext context)
		{
			Guard.ArgumentNotNull(() => profile);
			Guard.ArgumentNotNull(() => provider);
			Guard.ArgumentNotNull(() => context);

			var taskContext = new TaskExecutionContext(context, null);
			var cancellation = new CancellationTokenSource(TimeSpan.FromMinutes(5.0));

			taskContext.CancellationToken = cancellation.Token;

			InitDependencies(taskContext);

			var ctx = new ExportProfileTaskContext(taskContext, profile, provider, previewData: x => { });

			var unused = Init(ctx);

			var totalCount = ctx.RecordsPerStore.First().Value;

			return totalCount;
		}
	}
}
