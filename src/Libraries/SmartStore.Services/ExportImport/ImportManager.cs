using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Data;
using SmartStore.Services.Catalog;
using SmartStore.Core.Events;
using SmartStore.Services.Media;
using SmartStore.Services.Seo;
using SmartStore.Utilities;
using System.Text;

namespace SmartStore.Services.ExportImport
{
    /// <summary>
    /// Import manager
    /// </summary>
    public partial class ImportManager : IImportManager
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IPictureService _pictureService;
        private readonly IUrlRecordService _urlRecordService;
		private readonly IEventPublisher _eventPublisher;
		private readonly IRepository<Product> _rsProduct;
		private readonly IRepository<ProductCategory> _rsProductCategory;
		private readonly IRepository<ProductManufacturer> _rsProductManufacturer;
		private readonly IRepository<ProductPicture> _rsProductPicture;

        public ImportManager(
			IProductService productService, 
			ICategoryService categoryService,
            IManufacturerService manufacturerService, 
			IPictureService pictureService,
            IUrlRecordService urlRecordService,
			IEventPublisher eventPublisher,
			IRepository<Product> rsProduct,
			IRepository<ProductCategory> rsProductCategory,
			IRepository<ProductManufacturer> rsProductManufacturer,
			IRepository<ProductPicture> rsProductPicture)
        {
            this._productService = productService;
            this._categoryService = categoryService;
            this._manufacturerService = manufacturerService;
            this._pictureService = pictureService;
            this._urlRecordService = urlRecordService;
			this._eventPublisher = eventPublisher;
			this._rsProduct = rsProduct;
			this._rsProductCategory = rsProductCategory;
			this._rsProductManufacturer = rsProductManufacturer;
			this._rsProductPicture = rsProductPicture;
        }

		public virtual string CreateTextReport(ImportResult result)
		{
			var sb = new StringBuilder();

			using (var writer = new StringWriter(sb))
			{
				writer.WriteLine("SUMMARY");
				writer.WriteLine("==================================================================================");
				writer.WriteLine("Started: {0}".FormatCurrent(result.StartDateUtc.ToLocalTime()));
				writer.WriteLine("Finished: {0}{1}".FormatCurrent(result.EndDateUtc.ToLocalTime(), result.Cancelled ? " (cancelled by user)" : ""));
				writer.WriteLine("Duration: {0}".FormatCurrent((result.EndDateUtc - result.StartDateUtc).ToString("g")));

				writer.WriteLine("");
				writer.WriteLine("Total rows in source: {0}".FormatCurrent(result.TotalRecords));
				writer.WriteLine("Rows processed: {0}".FormatCurrent(result.AffectedRecords));
				writer.WriteLine("Products imported: {0}".FormatCurrent(result.NewRecords));
				writer.WriteLine("Products updated: {0}".FormatCurrent(result.ModifiedRecords));

				writer.WriteLine("");
				writer.WriteLine("Warnings: {0}".FormatCurrent(result.Messages.Count(x => x.MessageType == ImportMessageType.Warning)));
				writer.WriteLine("Errors: {0}".FormatCurrent(result.Messages.Count(x => x.MessageType == ImportMessageType.Error)));

				writer.WriteLine("");
				writer.WriteLine("");
				writer.WriteLine("MESSAGES");
				writer.WriteLine("==================================================================================");

				foreach (var message in result.Messages)
				{
					string msg = string.Empty;
					var prefix = new List<string>();
					if (message.AffectedItem != null)
					{
						prefix.Add("Pos: " + message.AffectedItem.Position + 1);
					}
					if (message.AffectedField.HasValue())
					{
						prefix.Add("Field: " + message.AffectedField);
					}

					if (prefix.Any())
					{
						msg = "[{0}] ".FormatCurrent(String.Join(", ", prefix));
					}

					msg += message.Message;

					writer.WriteLine("{0}: {1}".FormatCurrent(message.MessageType.ToString().ToUpper(), msg));
				}
			}

			return sb.ToString();
		}

		/// <summary>
		/// Import products from XLSX file
		/// </summary>
		/// <param name="stream">Stream</param>
		public virtual async Task<ImportResult> ImportProductsFromExcelAsync(
			Stream stream, 
			CancellationToken cancellationToken,
			IProgress<ImportProgressInfo> progress = null)
		{
			Guard.ArgumentNotNull(() => stream);

			var t = await Task.Run<ImportResult>(async () => {

				var result = new ImportResult();
				int saved = 0;

				using (var scope = new DbContextScope(ctx: _rsProduct.Context, autoDetectChanges: false, proxyCreation: false, validateOnSave: false))
				{
					using (var segmenter = new DataSegmenter<Product>(stream))
					{
						result.TotalRecords = segmenter.TotalRows;
						
						while (segmenter.ReadNextBatch() && !cancellationToken.IsCancellationRequested)
						{
							var batch = segmenter.CurrentBatch;

							// Update progress for calling thread
							if (progress != null)
							{
								progress.Report(new ImportProgressInfo
								{
									TotalRecords = result.TotalRecords,
									TotalProcessed = segmenter.CurrentSegmentFirstRowIndex - 1,
									NewRecords = result.NewRecords,
									ModifiedRecords = result.ModifiedRecords,
									ElapsedTime = DateTime.UtcNow - result.StartDateUtc,
									TotalWarnings = result.Messages.Count(x => x.MessageType == ImportMessageType.Warning),
									TotalErrors = result.Messages.Count(x => x.MessageType == ImportMessageType.Error),
								});
							}

							// ===========================================================================
							// 1.) Import products
							// ===========================================================================
							try
							{
								saved = await ProcessProducts(batch, result);
							}
							catch (Exception ex)
							{
								result.AddError(ex, segmenter.CurrentSegment, "ProcessProducts");
							}

							// reduce batch to saved (valid) products.
							// No need to perform import operations on errored products.
							batch = batch.Where(x => x.Entity != null && !x.IsTransient).AsReadOnly();

							// update result object
							result.NewRecords += batch.Count(x => x.IsNew && !x.IsTransient);
							result.ModifiedRecords += batch.Count(x => !x.IsNew && !x.IsTransient);

							// ===========================================================================
							// 2.) Import SEO Slugs
							// IMPORTANT: Unlike with Products AutoCommitEnabled must be TRUE,
							//            as Slugs are going to be validated against existing ones in DB.
							// ===========================================================================
							if (batch.Any(x => x.IsNew || (x.ContainsKey("SeName") || x.NameChanged)))
							{
								_rsProduct.Context.AutoDetectChangesEnabled = true;
								ProcessSlugs(batch, result);
								_rsProduct.Context.AutoDetectChangesEnabled = false;
							}

							// ===========================================================================
							// 3.) Import product category mappings
							// ===========================================================================
							if (batch.Any(x => x.ContainsKey("CategoryIds")))
							{
								try
								{
									await ProcessProductCategories(batch, result);
								}
								catch (Exception ex)
								{
									result.AddError(ex, segmenter.CurrentSegment, "ProcessProductCategories");
								}
							}

							// ===========================================================================
							// 4.) Import product manufacturer mappings
							// ===========================================================================
							if (batch.Any(x => x.ContainsKey("ManufacturerIds")))
							{
								try
								{
									await ProcessProductManufacturers(batch, result);
								}
								catch (Exception ex)
								{
									result.AddError(ex, segmenter.CurrentSegment, "ProcessProductManufacturers");
								}
							}

							// ===========================================================================
							// 5.) Import product picture mappings
							// ===========================================================================
							if (batch.Any(x => x.ContainsKey("Picture1") || x.ContainsKey("Picture2") || x.ContainsKey("Picture3")))
							{
								try
								{
									ProcessProductPictures(batch, result);
								}
								catch (Exception ex)
								{
									result.AddError(ex, segmenter.CurrentSegment, "ProcessProductPictures");
								}
							}

						}
					}
				}

				result.EndDateUtc = DateTime.UtcNow;

				if (cancellationToken.IsCancellationRequested)
				{
					result.Cancelled = true;
					result.AddInfo("Import task was cancelled by user");
				}

				return result;
			});

			return t;
		}

		private async Task<int> ProcessProducts(ICollection<ImportRow<Product>> batch, ImportResult result)
		{
			_rsProduct.AutoCommitEnabled = false;

			Product lastInserted = null;
			Product lastUpdated = null;

			foreach (var row in batch)
			{
				Product product = null;

				object key;

				// try get by int ID
				if (row.TryGetValue("Id", out key) && key.ToString().ToInt() > 0)
				{
					product = _productService.GetProductById(key.ToString().ToInt());
				}

				// try get by SKU
				if (product == null && row.TryGetValue("SKU", out key))
				{
					product = _productService.GetProductBySku(key.ToString());
				}

				// try get by GTIN
				if (product == null && row.TryGetValue("Gtin", out key))
				{
					product = _productService.GetProductByGtin(key.ToString());
				}

				if (product == null)
				{
					// a Name is required with new products.
					if (!row.ContainsKey("Name")) 
					{
						result.AddError("The 'Name' field is required for new products. Skipping row.", row.GetRowInfo(), "Name");
						continue;
					}
					product = new Product();
				}

				row.Initialize(product, row["Name"].ToString());

				if (!row.IsNew)
				{
					if (!product.Name.Equals(row["Name"].ToString(), StringComparison.OrdinalIgnoreCase))
					{
						// Perf: use this later for SeName updates.
						row.NameChanged = true;
					}
				}

				row.SetProperty(result, product, (x) => x.Sku);
				row.SetProperty(result, product, (x) => x.Gtin);
				row.SetProperty(result, product, (x) => x.ManufacturerPartNumber);
				row.SetProperty(result, product, (x) => x.ProductTypeId, (int)ProductType.SimpleProduct);
				row.SetProperty(result, product, (x) => x.ParentGroupedProductId);
				row.SetProperty(result, product, (x) => x.VisibleIndividually, true);
				row.SetProperty(result, product, (x) => x.Name);
				row.SetProperty(result, product, (x) => x.ShortDescription);
				row.SetProperty(result, product, (x) => x.FullDescription);
				row.SetProperty(result, product, (x) => x.ProductTemplateId);
				row.SetProperty(result, product, (x) => x.ShowOnHomePage);
				row.SetProperty(result, product, (x) => x.MetaKeywords);
				row.SetProperty(result, product, (x) => x.MetaDescription);
				row.SetProperty(result, product, (x) => x.MetaTitle);
				row.SetProperty(result, product, (x) => x.AllowCustomerReviews, true);
				row.SetProperty(result, product, (x) => x.Published, true);
				row.SetProperty(result, product, (x) => x.IsGiftCard);
				row.SetProperty(result, product, (x) => x.GiftCardTypeId);
				row.SetProperty(result, product, (x) => x.RequireOtherProducts);
				row.SetProperty(result, product, (x) => x.RequiredProductIds);
				row.SetProperty(result, product, (x) => x.AutomaticallyAddRequiredProducts);
				row.SetProperty(result, product, (x) => x.IsDownload);
				row.SetProperty(result, product, (x) => x.DownloadId);
				row.SetProperty(result, product, (x) => x.UnlimitedDownloads, true);
				row.SetProperty(result, product, (x) => x.MaxNumberOfDownloads, 10);
				row.SetProperty(result, product, (x) => x.DownloadActivationTypeId, 1);
				row.SetProperty(result, product, (x) => x.HasSampleDownload);
				row.SetProperty(result, product, (x) => x.SampleDownloadId, (int?)null, ZeroToNull);
				row.SetProperty(result, product, (x) => x.HasUserAgreement);
				row.SetProperty(result, product, (x) => x.UserAgreementText);
				row.SetProperty(result, product, (x) => x.IsRecurring);
				row.SetProperty(result, product, (x) => x.RecurringCycleLength, 100);
				row.SetProperty(result, product, (x) => x.RecurringCyclePeriodId);
				row.SetProperty(result, product, (x) => x.RecurringTotalCycles, 10);
				row.SetProperty(result, product, (x) => x.IsShipEnabled, true);
				row.SetProperty(result, product, (x) => x.IsFreeShipping);
				row.SetProperty(result, product, (x) => x.AdditionalShippingCharge);
				row.SetProperty(result, product, (x) => x.IsTaxExempt);
				row.SetProperty(result, product, (x) => x.TaxCategoryId, 1);
				row.SetProperty(result, product, (x) => x.ManageInventoryMethodId);
				row.SetProperty(result, product, (x) => x.StockQuantity, 10000);
				row.SetProperty(result, product, (x) => x.DisplayStockAvailability);
				row.SetProperty(result, product, (x) => x.DisplayStockQuantity);
				row.SetProperty(result, product, (x) => x.MinStockQuantity);
				row.SetProperty(result, product, (x) => x.LowStockActivityId);
				row.SetProperty(result, product, (x) => x.NotifyAdminForQuantityBelow, 1);
				row.SetProperty(result, product, (x) => x.BackorderModeId);
				row.SetProperty(result, product, (x) => x.AllowBackInStockSubscriptions);
				row.SetProperty(result, product, (x) => x.OrderMinimumQuantity, 1);
				row.SetProperty(result, product, (x) => x.OrderMaximumQuantity, 10000);
				row.SetProperty(result, product, (x) => x.AllowedQuantities);
				row.SetProperty(result, product, (x) => x.DisableBuyButton);
				row.SetProperty(result, product, (x) => x.DisableWishlistButton);
				row.SetProperty(result, product, (x) => x.AvailableForPreOrder);
				row.SetProperty(result, product, (x) => x.CallForPrice);
				row.SetProperty(result, product, (x) => x.Price);
				row.SetProperty(result, product, (x) => x.OldPrice);
				row.SetProperty(result, product, (x) => x.ProductCost);
				row.SetProperty(result, product, (x) => x.SpecialPrice);
				row.SetProperty(result, product, (x) => x.SpecialPriceStartDateTimeUtc, null, OADateToUtcDate);
				row.SetProperty(result, product, (x) => x.SpecialPriceEndDateTimeUtc, null, OADateToUtcDate);
				row.SetProperty(result, product, (x) => x.CustomerEntersPrice);
				row.SetProperty(result, product, (x) => x.MinimumCustomerEnteredPrice);
				row.SetProperty(result, product, (x) => x.MaximumCustomerEnteredPrice, 1000);
				row.SetProperty(result, product, (x) => x.Weight);
				row.SetProperty(result, product, (x) => x.Length);
				row.SetProperty(result, product, (x) => x.Width);
				row.SetProperty(result, product, (x) => x.Height);
				row.SetProperty(result, product, (x) => x.DeliveryTimeId);
				row.SetProperty(result, product, (x) => x.BasePriceEnabled);
				row.SetProperty(result, product, (x) => x.BasePriceMeasureUnit);
				row.SetProperty(result, product, (x) => x.BasePriceAmount);
				row.SetProperty(result, product, (x) => x.BasePriceBaseAmount);
				row.SetProperty(result, product, (x) => x.BundlePerItemPricing);
				row.SetProperty(result, product, (x) => x.BundlePerItemShipping);
				row.SetProperty(result, product, (x) => x.BundlePerItemShoppingCart);
				row.SetProperty(result, product, (x) => x.BundleTitleText);
				row.SetProperty(result, product, (x) => x.CreatedOnUtc, DateTime.UtcNow, OADateToUtcDate);

				product.UpdatedOnUtc = DateTime.UtcNow;

				if (row.IsTransient)
				{
					_rsProduct.Insert(product);
					lastInserted = product;
				}
				else
				{
					_rsProduct.Update(product);
					lastUpdated = product;
				}
			}

			// commit whole batch at once
			var t = await _rsProduct.Context.SaveChangesAsync();

			// Perf: notify only about LAST insertion and update
			if (lastInserted != null)
				_eventPublisher.EntityInserted(lastInserted);
			if (lastUpdated != null)
				_eventPublisher.EntityUpdated(lastUpdated);

			//// ensure all products got imported before processing other stuff.
			//t.Wait();

			return t;
		}

		private void ProcessSlugs(ICollection<ImportRow<Product>> batch, ImportResult result)
		{
			foreach (var row in batch)
			{
				if (row.IsNew || row.NameChanged || row.ContainsKey("SeName"))
				{
					try
					{
						string seName = row.GetValue<string>("SeName");
						_urlRecordService.SaveSlug(row.Entity, row.Entity.ValidateSeName(seName, row.Entity.Name, true), 0);
					}
					catch (Exception ex)
					{
						result.AddWarning(ex.Message, row.GetRowInfo(), "SeName");
					}
				}
			}
		}

		private async Task<int> ProcessProductCategories(ICollection<ImportRow<Product>> batch, ImportResult result)
		{
			_rsProductCategory.AutoCommitEnabled = false;

			ProductCategory lastInserted = null;
			
			foreach (var row in batch)
			{
				string categoryIds = row.GetValue<string>("CategoryIds");
				if (categoryIds.HasValue())
				{
					try
					{
						foreach (var id in categoryIds.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(x => Convert.ToInt32(x.Trim())))
						{
							if (_rsProductCategory.TableUntracked.Where(x => x.ProductId == row.Entity.Id && x.CategoryId == id).FirstOrDefault() == null)
							{
								// ensure that category exists
								var category = _categoryService.GetCategoryById(id);
								if (category != null)
								{
									var productCategory = new ProductCategory()
									{
										ProductId = row.Entity.Id,
										CategoryId = category.Id,
										IsFeaturedProduct = false,
										DisplayOrder = 1
									};
									_rsProductCategory.Insert(productCategory);
									lastInserted = productCategory;
								}
							}
						}
					}
					catch (Exception ex)
					{
						result.AddWarning(ex.Message, row.GetRowInfo(), "CategoryIds");
					}
				}
			}

			// commit whole batch at once
			var t = await _rsProductCategory.Context.SaveChangesAsync();

			// Perf: notify only about LAST insertion and update
			if (lastInserted != null)
				_eventPublisher.EntityInserted(lastInserted);

			return t;
		}

		private async Task<int> ProcessProductManufacturers(ICollection<ImportRow<Product>> batch, ImportResult result)
		{
			_rsProductManufacturer.AutoCommitEnabled = false;

			ProductManufacturer lastInserted = null;

			foreach (var row in batch)
			{
				string manufacturerIds = row.GetValue<string>("ManufacturerIds");
				if (manufacturerIds.HasValue())
				{
					try
					{
						foreach (var id in manufacturerIds.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(x => Convert.ToInt32(x.Trim())))
						{
							if (_rsProductManufacturer.TableUntracked.Where(x => x.ProductId == row.Entity.Id && x.ManufacturerId == id).FirstOrDefault() == null)
							{
								// ensure that manufacturer exists
								var manufacturer = _manufacturerService.GetManufacturerById(id);
								if (manufacturer != null)
								{
									var productManufacturer = new ProductManufacturer()
									{
										ProductId = row.Entity.Id,
										ManufacturerId = manufacturer.Id,
										IsFeaturedProduct = false,
										DisplayOrder = 1
									};
									_rsProductManufacturer.Insert(productManufacturer);
									lastInserted = productManufacturer;
								}
							}
						}
					}
					catch (Exception ex)
					{
						result.AddWarning(ex.Message, row.GetRowInfo(), "ManufacturerIds");
					}
				}
			}

			// commit whole batch at once
			var t = await _rsProductManufacturer.Context.SaveChangesAsync();

			// Perf: notify only about LAST insertion and update
			if (lastInserted != null)
				_eventPublisher.EntityInserted(lastInserted);

			return t;
		}

		private void ProcessProductPictures(ICollection<ImportRow<Product>> batch, ImportResult result)
		{
			// true, cause pictures must be saved and assigned an id
			// prior adding a mapping.
			_rsProductPicture.AutoCommitEnabled = true;

			ProductPicture lastInserted = null;

			foreach (var row in batch)
			{
				var pictures = new string[] 
				{
 					row.GetValue<string>("Picture1"),
					row.GetValue<string>("Picture2"),
					row.GetValue<string>("Picture3")
				};

				int i = 0;
				try
				{
					for (i = 0; i < pictures.Length; i++)
					{
						var picture = pictures[i];

						if (picture.IsEmpty() || !File.Exists(picture))
							continue;

						var currentPictures = _rsProductPicture.TableUntracked.Where(x => x.ProductId == row.Entity.Id);
						var pictureBinary = FindEqualPicture(picture, currentPictures);

						if (pictureBinary != null && pictureBinary.Length > 0)
						{
							// no equal picture found in sequence
							var newPicture = _pictureService.InsertPicture(pictureBinary, "image/jpeg", _pictureService.GetPictureSeName(row.EntityDisplayName), true, true);
							if (newPicture != null)
							{
								var mapping = new ProductPicture()
								{
									ProductId = row.Entity.Id,
									PictureId = newPicture.Id,
									DisplayOrder = 1,
								};
								_rsProductPicture.Insert(mapping);
								lastInserted = mapping;
							}
						}
						else
						{
							result.AddInfo("Found equal picture in data store. Skipping field.", row.GetRowInfo(), "Picture" + (i + 1).ToString());
						}
					}
				}
				catch (Exception ex)
				{
					result.AddWarning(ex.Message, row.GetRowInfo(), "Picture" + (i + 1).ToString());
				}
			
			}

			// Perf: notify only about LAST insertion and update
			if (lastInserted != null)
				_eventPublisher.EntityInserted(lastInserted);
		}

		private DateTime? OADateToUtcDate(object value)
		{
			double oaDate;
			if (CommonHelper.TryConvert<double>(value, out oaDate) && oaDate != 0)
			{
				return DateTime.FromOADate(Convert.ToDouble(oaDate));
			}

			return null;
		}


		private int? ZeroToNull(object value)
		{
			int result;
			if (CommonHelper.TryConvert<int>(value, out result) && result > 0)
			{
				return result;
			}

			return (int?)null;
		}

        /// <summary>
        /// Finds an equal picture by comparing the binary buffer
        /// </summary>
        /// <param name="path">The picture to find a duplicate for</param>
        /// <param name="productPictures">The sequence of product pictures to seek within for duplicates</param>
        /// <returns>The picture binary for <c>path</c> when no picture equals in the sequence, <c>null</c> otherwise.</returns>
        private byte[] FindEqualPicture(string path, IEnumerable<ProductPicture> productPictures)
        {
            try
            {
                var myBuffer = File.ReadAllBytes(path);

                foreach (var pictureMap in productPictures.Where(x => x.Id > 0))
                {
                    var otherBuffer = _pictureService.LoadPictureBinary(pictureMap.Picture);
                    using (var myStream = new MemoryStream(myBuffer))
                    {
                        using (var otherStream = new MemoryStream(otherBuffer))
                        {
                            var equals = myStream.ContentsEqual(otherStream);
                            if (equals)
                            {
                                return null;
                            }
                        }
                    }
                }

                return myBuffer;
            }
            catch
            {
                return null;
            }
		}
	}
}
