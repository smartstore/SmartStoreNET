using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Diagnostics;
using System.Reflection;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Services.Catalog;
using SmartStore.Services.Media;
using SmartStore.Services.Seo;
using OfficeOpenXml;
using Fasterflect;
using SmartStore.Services.Events;

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

		/// <summary>
        /// Import products from XLSX file
        /// </summary>
        /// <param name="stream">Stream</param>
		public virtual void ImportProductsFromXlsx(Stream stream)
		{
			Guard.ArgumentNotNull(() => stream);

			using (var scope = new DbContextScope(autoDetectChanges: false, proxyCreation: false, validateOnSave: false))
			{
				using (var segmenter = new DataSegmenter<Product>(stream))
				{
					while (segmenter.ReadNextBatch())
					{
						var batch = segmenter.CurrentBatch;

						// ===========================================================================
						// 1.) Import products
						// ===========================================================================
						try
						{
							ProcessProducts(batch);
						}
						catch (Exception ex)
						{
							Console.WriteLine(ex.Message);
						}

						// reduce batch to saved (valid) products.
						// No need to perform import operations on errored products.
						batch = batch.Where(x => !x.IsTransient).AsReadOnly();

						// ===========================================================================
						// 2.) Import SEO Slugs
						// IMPORTANT: Unlike with Products AutoCommitEnabled must be TRUE,
						//            as Slugs are going to be validated against existing ones in DB.
						// ===========================================================================
						if (batch.Any(x => x.IsNew || (x.ContainsKey("SeName") || x.NameChanged)))
						{
							try
							{
								ProcessSlugs(batch);
							}
							catch (Exception ex)
							{
								Console.WriteLine(ex.Message);
							}
						}

						// ===========================================================================
						// 3.) Import product category mappings
						// ===========================================================================
						if (batch.Any(x => x.ContainsKey("CategoryIds")))
						{
							try
							{
								ProcessProductCategories(batch);
							}
							catch (Exception ex)
							{
								Console.WriteLine(ex.Message);
							}
						}

						// ===========================================================================
						// 4.) Import product manufacturer mappings
						// ===========================================================================
						if (batch.Any(x => x.ContainsKey("ManufacturerIds")))
						{
							try
							{
								ProcessProductManufacturers(batch);
							}
							catch (Exception ex)
							{
								Console.WriteLine(ex.Message);
							}
						}

						// ===========================================================================
						// 5.) Import product picture mappings
						// ===========================================================================
						if (batch.Any(x => x.ContainsKey("Picture1") || x.ContainsKey("Picture2") || x.ContainsKey("Picture3")))
						{
							try
							{
								ProcessProductPictures(batch);
							}
							catch (Exception ex)
							{
								Console.WriteLine(ex.Message);
							}
						}

					}
				}
			}
		}

		private void ProcessProducts(ICollection<Row<Product>> batch)
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
						// TBD: (MSG) For new products, the "Name" field is required.
						continue;
					}
					product = new Product();
				}

				row.Initialize(product, product.Name);

				if (!row.IsNew)
				{
					if (!product.Name.Equals(row["Name"].ToString(), StringComparison.OrdinalIgnoreCase))
					{
						// Perf: use this later for SeName updates.
						row.NameChanged = true;
					}
				}

				row.SetProperty(product, (x) => x.Sku);
				row.SetProperty(product, (x) => x.Gtin);
				row.SetProperty(product, (x) => x.ManufacturerPartNumber);
				row.SetProperty(product, (x) => x.ProductTypeId, (int)ProductType.SimpleProduct);
				row.SetProperty(product, (x) => x.ParentGroupedProductId);
				row.SetProperty(product, (x) => x.VisibleIndividually, true);
				row.SetProperty(product, (x) => x.Name);
				row.SetProperty(product, (x) => x.ShortDescription);
				row.SetProperty(product, (x) => x.FullDescription);
				row.SetProperty(product, (x) => x.ProductTemplateId, 1 /* TODO: dyn */);
				row.SetProperty(product, (x) => x.ShowOnHomePage);
				row.SetProperty(product, (x) => x.MetaKeywords);
				row.SetProperty(product, (x) => x.MetaDescription);
				row.SetProperty(product, (x) => x.MetaTitle);
				row.SetProperty(product, (x) => x.AllowCustomerReviews, true);
				row.SetProperty(product, (x) => x.Published, true);
				row.SetProperty(product, (x) => x.IsGiftCard);
				row.SetProperty(product, (x) => x.GiftCardTypeId);
				row.SetProperty(product, (x) => x.RequireOtherProducts);
				row.SetProperty(product, (x) => x.RequiredProductIds);
				row.SetProperty(product, (x) => x.AutomaticallyAddRequiredProducts);
				row.SetProperty(product, (x) => x.IsDownload);
				row.SetProperty(product, (x) => x.DownloadId);
				row.SetProperty(product, (x) => x.UnlimitedDownloads, true);
				row.SetProperty(product, (x) => x.MaxNumberOfDownloads, 10);
				row.SetProperty(product, (x) => x.DownloadActivationTypeId, 1);
				row.SetProperty(product, (x) => x.HasSampleDownload);
				row.SetProperty(product, (x) => x.SampleDownloadId);
				row.SetProperty(product, (x) => x.HasUserAgreement);
				row.SetProperty(product, (x) => x.UserAgreementText);
				row.SetProperty(product, (x) => x.IsRecurring);
				row.SetProperty(product, (x) => x.RecurringCycleLength, 100);
				row.SetProperty(product, (x) => x.RecurringCyclePeriodId);
				row.SetProperty(product, (x) => x.RecurringTotalCycles, 10);
				row.SetProperty(product, (x) => x.IsShipEnabled, true);
				row.SetProperty(product, (x) => x.IsFreeShipping);
				row.SetProperty(product, (x) => x.AdditionalShippingCharge);
				row.SetProperty(product, (x) => x.IsTaxExempt);
				row.SetProperty(product, (x) => x.TaxCategoryId, 1 /* TODO: dyn */);
				row.SetProperty(product, (x) => x.ManageInventoryMethodId);
				row.SetProperty(product, (x) => x.StockQuantity, 10000);
				row.SetProperty(product, (x) => x.DisplayStockAvailability);
				row.SetProperty(product, (x) => x.DisplayStockQuantity);
				row.SetProperty(product, (x) => x.MinStockQuantity);
				row.SetProperty(product, (x) => x.LowStockActivityId);
				row.SetProperty(product, (x) => x.NotifyAdminForQuantityBelow, 1);
				row.SetProperty(product, (x) => x.BackorderModeId);
				row.SetProperty(product, (x) => x.AllowBackInStockSubscriptions);
				row.SetProperty(product, (x) => x.OrderMinimumQuantity, 1);
				row.SetProperty(product, (x) => x.OrderMaximumQuantity, 10000);
				row.SetProperty(product, (x) => x.AllowedQuantities);
				row.SetProperty(product, (x) => x.DisableBuyButton);
				row.SetProperty(product, (x) => x.DisableWishlistButton);
				row.SetProperty(product, (x) => x.AvailableForPreOrder);
				row.SetProperty(product, (x) => x.CallForPrice);
				row.SetProperty(product, (x) => x.Price);
				row.SetProperty(product, (x) => x.OldPrice);
				row.SetProperty(product, (x) => x.ProductCost);
				row.SetProperty(product, (x) => x.SpecialPrice/*, null, DoubleToDecimal*/);
				row.SetProperty(product, (x) => x.SpecialPriceStartDateTimeUtc, null, OADateToUtcDate);
				row.SetProperty(product, (x) => x.SpecialPriceEndDateTimeUtc, null, OADateToUtcDate);
				row.SetProperty(product, (x) => x.CustomerEntersPrice);
				row.SetProperty(product, (x) => x.MinimumCustomerEnteredPrice);
				row.SetProperty(product, (x) => x.MaximumCustomerEnteredPrice, 1000);
				row.SetProperty(product, (x) => x.Weight);
				row.SetProperty(product, (x) => x.Length);
				row.SetProperty(product, (x) => x.Width);
				row.SetProperty(product, (x) => x.Height);
				row.SetProperty(product, (x) => x.DeliveryTimeId/*, null, DoubleToInt*/);
				row.SetProperty(product, (x) => x.BasePrice_Enabled);
				row.SetProperty(product, (x) => x.BasePrice_MeasureUnit);
				row.SetProperty(product, (x) => x.BasePrice_Amount/*, null, DoubleToDecimal*/);
				row.SetProperty(product, (x) => x.BasePrice_BaseAmount/*, null, DoubleToInt*/);
				row.SetProperty(product, (x) => x.CreatedOnUtc, DateTime.UtcNow, OADateToUtcDate);

				product.UpdatedOnUtc = DateTime.UtcNow;

				// Offen: SeName, CategoryIds, ManufacturerIds, Picture1, Picture2, Picture3

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
			_rsProduct.Context.SaveChanges();

			// Perf: notify only about LAST insertion and update
			if (lastInserted != null)
				_eventPublisher.EntityInserted(lastInserted);
			if (lastUpdated != null)
				_eventPublisher.EntityUpdated(lastUpdated);
		}

		private void ProcessSlugs(ICollection<Row<Product>> batch)
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
						Console.Write("TBD: (MSG)");
					}
				}
			}
		}

		private void ProcessProductCategories(ICollection<Row<Product>> batch)
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
						Console.Write("TBD: (MSG)");
					}
				}
			}

			// commit whole batch at once
			_rsProductCategory.Context.SaveChanges();

			// Perf: notify only about LAST insertion and update
			if (lastInserted != null)
				_eventPublisher.EntityInserted(lastInserted);
		}

		private void ProcessProductManufacturers(ICollection<Row<Product>> batch)
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
						Console.Write("TBD: (MSG)");
					}
				}
			}

			// commit whole batch at once
			_rsProductManufacturer.Context.SaveChanges();

			// Perf: notify only about LAST insertion and update
			if (lastInserted != null)
				_eventPublisher.EntityInserted(lastInserted);
		}

		private void ProcessProductPictures(ICollection<Row<Product>> batch)
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
				}.Where(x => x.HasValue());

				try
				{
					foreach (var picture in pictures)
					{
						if (!File.Exists(picture))
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
					}
				}
				catch (Exception ex)
				{
					Console.Write("TBD: (MSG)");
				}
			
			}

			// Perf: notify only about LAST insertion and update
			if (lastInserted != null)
				_eventPublisher.EntityInserted(lastInserted);
		}

		private DateTime? OADateToUtcDate(object value)
		{
			double oaDate;
			if (value.TryConvert<double>(out oaDate) && oaDate != 0)
			{
				return DateTime.FromOADate(Convert.ToDouble(oaDate));
			}

			return null;
		}

        /// <summary>
        /// Finds an equal picture by comparing the binary buffer
        /// </summary>
        /// <param name="path">The picture to find a duplicate for</param>
        /// <param name="productPictures">The sequence of product pictures to seek within for duplicates</param>
        /// <returns>The picture binary for <c>path</c> when no picture euqals in the sequence, <c>null</c> otherwise.</returns>
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

		#region Nested classes

		private class TargetProperty
		{
			public bool IsSettable { get; set; }
			public PropertyInfo PropertyInfo { get; set; }
		}

		private class Row<T> : Dictionary<string, object> where T : BaseEntity
		{
			private bool _initialized = false;
			private T _entity;
			private string _entityDisplayName;
			private int _position;
			private bool _isNew;

			public Row(string[] columns, object[] values, IDictionary<string, TargetProperty> properties, int position)
				: base(columns.Length, StringComparer.InvariantCultureIgnoreCase)
			{
				_position = position;

				for (int i = 0; i < columns.Length; i++)
				{
					var col = columns[i];
					var val = values[i];

					if (val != null && val.ToString().HasValue())
					{
						if (!properties.ContainsKey(col) || properties[col].IsSettable)
						{
							// only add value when no correponding property exists (special field)
							// or when property exists but it's publicly settable.
							this[col] = val;
						}
					}
				}
			}

			public void Initialize(T entity, string entityDisplayName)
			{
				_entity = entity;
				_entityDisplayName = entityDisplayName;
				_isNew = _entity.Id == 0;

				_initialized = true;
			}

			private void CheckInitialized()
			{
				if (_initialized)
				{
					throw Error.InvalidOperation("A row must be initialized before interacting with the entity or the data store");
				}
			}

			public bool IsTransient
			{
				get { return _entity.Id == 0; }
			}

			public bool IsNew
			{
				get { return _isNew; }
			}

			public T Entity
			{
				get { return _entity; }
			}

			public string EntityDisplayName
			{
				get { return _entityDisplayName; }
			}

			public bool NameChanged
			{
				get;
				set;
			}

			public int Position
			{
				get { return _position; }
			}

			public TProp GetValue<TProp>(string columnName)
			{
				object value;
				if (this.TryGetValue(columnName, out value))
				{
					return value.Convert<TProp>();
				}

				return default(TProp);
			}

			public bool SetProperty<TProp>(T target, Expression<Func<Product, TProp>> prop, TProp defaultValue = default(TProp), Func<object, TProp> converter = null)
			{
				// TBD: (MC) do not check for perf reason?
				//CheckInitialized();
				
				var pi = prop.ExtractPropertyInfo();
				var propName = pi.Name;

				object value;
				if (this.TryGetValue(propName, out value))
				{
					// source contains field value. Set it.
					TProp converted;
					if (converter != null)
					{
						converted = converter(value);
					}
					else
					{
						converted = value.Convert<TProp>();
					}
					return target.TrySetPropertyValue(propName, converted);
				}
				else
				{
					// source does not contain field data or it's empty...
					if (IsTransient && defaultValue != null /*!defaultValue.Equals(default(TProp))*/)
					{
						// ...but the entity is new. In this case
						// set the default value if given.
						return target.TrySetPropertyValue(propName, defaultValue);
					}
				}

				return false;
			}
		}

		private class DataSegmenter<T> : DisposableObject where T : BaseEntity
		{
			private const int BATCHSIZE = 100;
			
			private ExcelPackage _excelPackage;
			private ExcelWorksheet _sheet;
			private int _totalRows;
			private int _totalColumns;
			private readonly string[] _columns;
			private readonly IDictionary<string, TargetProperty> _properties;
			private IList<Row<T>> _currentBatch;
			private IPageable _pageable;
			private bool _bof;

			public DataSegmenter(Stream source)
			{
				Guard.ArgumentNotNull(() => source);

				_excelPackage = new ExcelPackage(source);

				// get the first worksheet in the workbook
				_sheet = _excelPackage.Workbook.Worksheets.FirstOrDefault();
				if (_sheet == null)
				{
					throw Error.InvalidOperation("The excel package does not contain any worksheet.");
				}

				if (_sheet.Dimension == null)
				{
					throw Error.InvalidOperation("The excel worksheet does not contain any data.");
				}

				_totalColumns = _sheet.Dimension.End.Column;
				_totalRows = _sheet.Dimension.End.Row - 1; // excluding 1st

				// Determine column names from 1st row (excel indexes start from 1)
				var cols = new List<string>();
				for (int i = 1; i <= _totalColumns; i++)
				{
					cols.Add(_sheet.Cells[1, i].Text);
				}

				_columns = cols.ToArray();
				ValidateColumns(_columns);
				_properties = new Dictionary<string, TargetProperty>(_columns.Length, StringComparer.InvariantCultureIgnoreCase);

				// determine corresponding Properties for given columns 
				var t = typeof(T);
				foreach (var col in _columns)
				{
					var pi = t.GetProperty(col);
					if (pi != null)
					{
						_properties[col] = new TargetProperty
						{
							IsSettable = pi.CanWrite && pi.GetSetMethod().IsPublic,
							PropertyInfo = pi
						};
					}
				}

				_bof = true;
				_pageable = new Pageable(0, BATCHSIZE, _totalRows);
			}

			public int TotalRows
			{
				get { return _totalRows; }
			}

			public int TotalColumns
			{
				get { return _totalColumns; }
			}

			public int CurrentSegment
			{
				get { return _pageable.PageNumber; }
			}

			public int TotalSegments
			{
				get { return _pageable.TotalPages; }
			}

			public void Reset()
			{
				if (_pageable.PageIndex != 0 && _currentBatch != null)
				{
					_currentBatch.Clear();
					_currentBatch = null;
				}
				_bof = true;
				_pageable.PageIndex = 0;
			}

			public bool ReadNextBatch()
			{
				if (_currentBatch != null)
				{
					_currentBatch.Clear();
					_currentBatch = null;
				}

				if (_bof)
				{
					_bof = false;
					return _pageable.TotalCount > 0;
				}

				if (_pageable.HasNextPage)
				{
					_pageable.PageIndex++;
					return true;
				}

				Reset();
				return false;
			}

			public ICollection<Row<T>> CurrentBatch
			{
				get
				{
					if (_currentBatch == null)
					{
						_currentBatch = new List<Row<T>>();

						int start = _pageable.FirstItemIndex + 1;
						int end = _pageable.LastItemIndex + 1;

						// Determine cell values per row
						for (int r = start; r <= end; r++)
						{
							var values = new List<object>();
							for (int c = 1; c <= _totalColumns; c++)
							{
								values.Add(_sheet.Cells[r, c].Value);
							}

							_currentBatch.Add(new Row<T>(_columns, values.ToArray(), _properties, r - 1));
						}
					}

					return _currentBatch.AsReadOnly();
				}
			}

			protected override void OnDispose(bool disposing)
			{
				if (disposing)
				{
					_sheet = null;
					if (_excelPackage != null)
					{
						_excelPackage.Dispose();
						_excelPackage = null;
					}
				}
			}

			private void ValidateColumns(string[] columns)
			{
				if (columns.Any(x => x.IsEmpty()))
				{
					throw Error.InvalidOperation("The first row must contain the column names and therefore cannot have empty cells.");
				}

				if (columns.Select(x => x.ToLower()).Distinct().ToArray().Length != columns.Length)
				{
					throw Error.InvalidOperation("The first row cannot contain duplicate column names.");
				}
			}
		}


		#endregion
	}
}
