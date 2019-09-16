using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using SmartStore.Collections;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Cms;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Configuration;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Domain.Security;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.IO;
using SmartStore.Core.Security;
using SmartStore.Data.Setup;
using SmartStore.Utilities;
using EfState = System.Data.Entity.EntityState;

namespace SmartStore.Data.Utilities
{
    public static class DataMigrator
	{
        #region Download.ProductId

        /// <summary>
        /// Sets EntityId &  EntityName for download table
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static int SetDownloadProductId(IDbContext context)
        {
            var ctx = context as SmartObjectContext;
            if (ctx == null)
                throw new ArgumentException("Passed context must be an instance of type '{0}'.".FormatInvariant(typeof(SmartObjectContext)), nameof(context));

			const string entityName = "Product";

#pragma warning disable 612, 618
            // Get all products with a download 
            var productQuery = from p in ctx.Set<Product>().AsNoTracking()
                        where (p.DownloadId != 0)
                        orderby p.Id
                        select new { p.Id, p.DownloadId };
#pragma warning restore 612, 618

            var downloads = context.Set<Download>().Select(x => x).ToDictionary(x => x.Id);
            
            int pageIndex = -1;
            while (true)
            {
                var products = PagedList.Create(productQuery, ++pageIndex, 1000);
                
                foreach (var p in products)
                {
                    try
                    {
                        if (downloads.TryGetValue(p.DownloadId, out var download))
                        {
                            download.EntityId = p.Id;
                            download.EntityName = entityName;
                        }
                    }
                    catch { }
                }

                context.SaveChanges();

                if (!products.HasNextPage)
                    break;
            }

            return 0;
        }

        #endregion

        #region Product.MainPicture

        /// <summary>
        /// Fixes 'MainPictureId' property of a single product entity
        /// </summary>
        /// <param name="context">Database context (must be <see cref="SmartObjectContext"/>)</param>
        /// <param name="entities">When <c>null</c>, Product.ProductPictures gets called.</param>
        /// <param name="product">Product to fix</param>
        /// <returns><c>true</c> when value was fixed</returns>
        public static bool FixProductMainPictureId(IDbContext context, Product product, IEnumerable<ProductPicture> entities = null)
		{
			Guard.NotNull(product, nameof(product));

			// INFO: this method must be able to handle pre-save state also.

			var ctx = context as SmartObjectContext;
			if (ctx == null)
				throw new ArgumentException("Passed context must be an instance of type '{0}'.".FormatInvariant(typeof(SmartObjectContext)), nameof(context));

			entities = entities ?? product.ProductPictures;
			if (entities == null)
				return false;

			var transientEntities = entities.Where(x => x.Id == 0);

			var sortedEntities = entities
				// Remove transient entities
				.Except(transientEntities) 
				.OrderBy(x => x.DisplayOrder)
				.ThenBy(x => x.Id)
				.Select(x => ctx.Entry(x))
				// Remove deleted and detached entities
				.Where(x => x.State != EfState.Deleted && x.State != EfState.Detached) 
				.Select(x => x.Entity)
				// Added/transient entities must be appended
				.Concat(transientEntities.OrderBy(x => x.DisplayOrder));

			var newMainPictureId = sortedEntities.FirstOrDefault()?.PictureId;

			if (newMainPictureId != product.MainPictureId)
			{
				product.MainPictureId = newMainPictureId;
				return true;
			}

			return false;
		}

		/// <summary>
		/// Traverses all products and fixes 'MainPictureId' property values if it is out of sync.
		/// </summary>
		/// <param name="context">Database context (must be <see cref="SmartObjectContext"/>)</param>
		/// <param name="ifModifiedSinceUtc">Minimum modified or created date of products to process. Pass <c>null</c> to fix all products.</param>
		/// <returns>The total count of fixed and updated product entities</returns>
		public static int FixProductMainPictureIds(IDbContext context, DateTime? ifModifiedSinceUtc = null)
		{
			return FixProductMainPictureIds(context, false, ifModifiedSinceUtc);
		}

		/// <summary>
		/// Called from migration seeder and only processes product entities without MainPictureId value.
		/// </summary>
		/// <returns>The total count of fixed and updated product entities</returns>
		internal static int FixProductMainPictureIds(IDbContext context, bool initial, DateTime? ifModifiedSinceUtc = null)
		{
			var ctx = context as SmartObjectContext;
			if (ctx == null)
				throw new ArgumentException("Passed context must be an instance of type '{0}'.".FormatInvariant(typeof(SmartObjectContext)), nameof(context));

			var query = from p in ctx.Set<Product>().AsNoTracking()
						where (!initial || p.MainPictureId == null) && (ifModifiedSinceUtc == null || p.UpdatedOnUtc >= ifModifiedSinceUtc.Value)
						orderby p.Id
						select new { p.Id, p.MainPictureId };	

			// Key = ProductId, Value = MainPictureId
			var toUpdate = new Dictionary<int, int?>();

			// 1st pass
			int pageIndex = -1;
			while (true)
			{
				var products = PagedList.Create(query, ++pageIndex, 1000);
				var map = GetPoductPictureMap(ctx, products.Select(x => x.Id).ToArray());

				foreach (var p in products)
				{
					int? fixedPictureId = null;
					if (map.ContainsKey(p.Id))
					{
						// Product has still a pic.
						fixedPictureId = map[p.Id];
					}

					// Update only if fixed PictureId differs from current
					if (fixedPictureId != p.MainPictureId)
					{
						toUpdate.Add(p.Id, fixedPictureId);
					}
				}

				if (!products.HasNextPage)
					break;
			}

			// 2nd pass
			foreach (var chunk in toUpdate.Slice(1000))
			{
				using (var tx = ctx.Database.BeginTransaction())
				{
					foreach (var kvp in chunk)
					{
						context.ExecuteSqlCommand("Update [Product] Set [MainPictureId] = {0} WHERE [Id] = {1}", false, null, kvp.Value, kvp.Key);
					}

					context.SaveChanges();
					tx.Commit();
				}
			}

			return toUpdate.Count;
		}

		private static IDictionary<int, int> GetPoductPictureMap(SmartObjectContext context, IEnumerable<int> productIds)
		{
			var map = new Dictionary<int, int>();

			var query = from pp in context.Set<ProductPicture>().AsNoTracking()
						where productIds.Contains(pp.ProductId)
						group pp by pp.ProductId into g
						select new
						{
							ProductId = g.Key,
							PictureIds = g.OrderBy(x => x.DisplayOrder).ThenBy(x => x.Id)
								.Take(1)
								.Select(x => x.PictureId)
						};

			map = query.ToList().ToDictionary(x => x.ProductId, x => x.PictureIds.First());

			return map;
		}

		#endregion

		#region MoveFsMedia (V3.1)

		/// <summary>
		/// Reorganizes media files in subfolders for V3.1
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		public static int MoveFsMedia(IDbContext context)
		{
			var ctx = context as SmartObjectContext;
			if (ctx == null)
				throw new ArgumentException("Passed context must be an instance of type '{0}'.".FormatInvariant(typeof(SmartObjectContext)), nameof(context));

			int dirMaxLength = 4;

			// Check whether FS storage provider is active...
			var setting = context.Set<Setting>().FirstOrDefault(x => x.Name == "Media.Storage.Provider");
			if (setting == null || !setting.Value.IsCaseInsensitiveEqual("MediaStorage.SmartStoreFileSystem"))
			{
				// DB provider is active: no need to move anything.
				return 0;
			}

			// What a huge, fucking hack! > IMediaFileSystem is defined in an
			// assembly which we don't reference from here. But it also implements
			// IFileSystem, which we can cast to.
			var fsType = Type.GetType("SmartStore.Services.Media.IMediaFileSystem, SmartStore.Services");
			var fs = EngineContext.Current.Resolve(fsType) as IFileSystem;

			// Pattern for file matching. E.g. matches 0000234-0.png
			var rg = new Regex(@"^([0-9]{7})-0[.](.{3,4})$", RegexOptions.Compiled | RegexOptions.Singleline);
			var subfolders = new Dictionary<string, string>();
			int i = 0;

			// Get root files
			var files = fs.ListFiles("");
			foreach (var chunk in files.Slice(500))
			{
				foreach (var file in chunk)
				{
					var match = rg.Match(file.Name);
					if (match.Success)
					{
						var name = match.Groups[1].Value;
						var ext = match.Groups[2].Value;
						// The new file name without trailing -0
						var newName = string.Concat(name, ".", ext);
						// The subfolder name, e.g. 0024, when file name is 0024893.png
						var dirName = name.Substring(0, dirMaxLength);

						if (!subfolders.TryGetValue(dirName, out string subfolder))
						{
							// Create subfolder "Storage/0000"
							subfolder = fs.Combine("Storage", dirName);
							fs.TryCreateFolder(subfolder);
							subfolders[dirName] = subfolder;
						}

						// Build destination path
						var destinationPath = fs.Combine(subfolder, newName);

						// Move the file now!
						fs.RenameFile(file.Path, destinationPath);
						i++;
					}
				}
			}

			return i;
		}

		#endregion

		#region Address Formats

		public static int ImportAddressFormats(IDbContext context)
		{
			var ctx = context as SmartObjectContext;
			if (ctx == null)
				throw new ArgumentException("Passed context must be an instance of type '{0}'.".FormatInvariant(typeof(SmartObjectContext)), nameof(context));

			var filePath = CommonHelper.MapPath("~/App_Data/AddressFormats.xml");

			if (!File.Exists(filePath))
			{
				return 0;
			}

			var countries = ctx.Set<Country>()
				.Where(x => x.AddressFormat == null)
				.ToList()
				.ToDictionarySafe(x => x.TwoLetterIsoCode, StringComparer.OrdinalIgnoreCase);

			var doc = XDocument.Load(filePath);

			foreach (var node in doc.Root.Nodes().OfType<XElement>())
			{
				var code = node.Attribute("code")?.Value?.Trim();
				var format = node.Value.Trim();

				if (code.HasValue() && countries.TryGetValue(code, out var country))
				{
					country.AddressFormat = format;
				}
			}

			return ctx.SaveChanges();
		}

		#endregion

		#region MoveCustomerFields (V3.2)

		/// <summary>
		/// Moves several customer fields saved as generic attributes to customer entity (Title, FirstName, LastName, BirthDate, Company, CustomerNumber)
		/// </summary>
		/// <param name="context">Database context (must be <see cref="SmartObjectContext"/>)</param>
		/// <returns>The total count of fixed and updated customer entities</returns>
		public static int MoveCustomerFields(IDbContext context)
		{
			var ctx = context as SmartObjectContext;
			if (ctx == null)
				throw new ArgumentException("Passed context must be an instance of type '{0}'.".FormatInvariant(typeof(SmartObjectContext)), nameof(context));

			// We delete attrs only if the WHOLE migration succeeded
			var attrIdsToDelete = new List<int>(1000);
			var gaTable = context.Set<GenericAttribute>();
			var candidates = new[] { "Title", "FirstName", "LastName", "Company", "CustomerNumber", "DateOfBirth" };

			var query = gaTable
				.AsNoTracking()
				.Where(x => x.KeyGroup == "Customer" && candidates.Contains(x.Key))
				.OrderBy(x => x.Id);

			int numUpdated = 0;

			using (var scope = new DbContextScope(ctx: context, validateOnSave: false, hooksEnabled: false, autoCommit: false))
			{
				for (var pageIndex = 0; pageIndex < 9999999; ++pageIndex)
				{
					var attrs = new PagedList<GenericAttribute>(query, pageIndex, 250);

					var customerIds = attrs.Select(a => a.EntityId).Distinct().ToArray();
					var customers = context.Set<Customer>()
						.Where(x => customerIds.Contains(x.Id))
						.ToDictionary(x => x.Id);

					// Move attrs one by one to customer
					foreach (var attr in attrs)
					{
						var customer = customers.Get(attr.EntityId);
						if (customer == null)
							continue;

						switch (attr.Key)
						{
							case "Title":
								customer.Title = attr.Value?.Truncate(100);
								break;
							case "FirstName":
								customer.FirstName = attr.Value?.Truncate(225);
								break;
							case "LastName":
								customer.LastName = attr.Value?.Truncate(225);
								break;
							case "Company":
								customer.Company = attr.Value?.Truncate(255);
								break;
							case "CustomerNumber":
								customer.CustomerNumber = attr.Value?.Truncate(100);
								break;
							case "DateOfBirth":
								customer.BirthDate = attr.Value?.Convert<DateTime?>();
								break;
						}

						// Update FullName
						var parts = new[] { customer.Title, customer.FirstName, customer.LastName };
						customer.FullName = string.Join(" ", parts.Where(x => x.HasValue())).NullEmpty();

						attrIdsToDelete.Add(attr.Id);
					}

					// Save batch
					numUpdated += scope.Commit();

					// Breathe
					context.DetachAll();

					if (!attrs.HasNextPage)
						break;
				}

				// Everything worked out, now delete all orpahned attributes
				if (attrIdsToDelete.Count > 0)
				{
					try
					{
						// Don't rollback migration when this fails
						var stubs = attrIdsToDelete.Select(x => new GenericAttribute { Id = x }).ToList();
						foreach (var chunk in stubs.Slice(500))
						{
							chunk.Each(x => gaTable.Attach(x));
							gaTable.RemoveRange(chunk);
							scope.Commit();
						}
					}
					catch (Exception ex)
					{
						var msg = ex.Message;
					}
				}
			}

			return numUpdated;
		}


        /// <summary>
        /// Moves several customer fields saved as generic attributes to customer entity 
        /// (Gender, ZipPostalCode, VatNumberStatusId, TimeZoneId, TaxDisplayTypeId, CountryId, CurrencyId, LanguageId, LastForumVisit, LastUserAgent)
        /// </summary>
        /// <param name="context">Database context (must be <see cref="SmartObjectContext"/>)</param>
        /// <returns>The total count of fixed and updated customer entities</returns>
        public static int MoveFurtherCustomerFields(IDbContext context)
        {
            var ctx = context as SmartObjectContext;
            if (ctx == null)
                throw new ArgumentException("Passed context must be an instance of type '{0}'.".FormatInvariant(typeof(SmartObjectContext)), nameof(context));

            // We delete attrs only if the WHOLE migration succeeded
            var attrIdsToDelete = new List<int>(1000);
            var gaTable = context.Set<GenericAttribute>();
            var candidates = new[] { "Gender", "VatNumberStatusId", "TimeZoneId", "TaxDisplayTypeId", "LastForumVisit", "LastUserAgent", "LastUserDeviceType" };

            var query = gaTable
                .AsNoTracking()
                .Where(x => x.KeyGroup == "Customer" && candidates.Contains(x.Key))
                .OrderBy(x => x.Id);

            int numUpdated = 0;

            using (var scope = new DbContextScope(ctx: context, validateOnSave: false, hooksEnabled: false, autoCommit: false))
            {
                for (var pageIndex = 0; pageIndex < 9999999; ++pageIndex)
                {
                    var attrs = new PagedList<GenericAttribute>(query, pageIndex, 250);

                    var customerIds = attrs.Select(a => a.EntityId).Distinct().ToArray();
                    var customers = context.Set<Customer>()
                        .Where(x => customerIds.Contains(x.Id))
                        .ToDictionary(x => x.Id);

                    // Move attrs one by one to customer
                    foreach (var attr in attrs)
                    {
                        var customer = customers.Get(attr.EntityId);
                        if (customer == null)
                            continue;

                        switch (attr.Key)
                        {
                            case "Gender":
                                customer.Gender = attr.Value?.Truncate(100);
                                break;
                            case "VatNumberStatusId":
                                customer.VatNumberStatusId = attr.Value.Convert<int>();
                                break;
                            case "TimeZoneId":
                                customer.TimeZoneId = attr.Value?.Truncate(255);
                                break;
                            case "TaxDisplayTypeId":
                                customer.TaxDisplayTypeId = attr.Value.Convert<int>();
                                break;
                            case "LastForumVisit":
                                customer.LastForumVisit = attr.Value.Convert<DateTime>();
                                break;
                            case "LastUserAgent":
                                customer.LastUserAgent = attr.Value.Convert<string>();
                                break;
                            case "LastUserDeviceType":
                                // TODO: split
                                customer.LastUserDeviceType = attr.Value.Convert<string>();
                                break;
                        }

                        attrIdsToDelete.Add(attr.Id);
                    }

                    // Save batch
                    numUpdated += scope.Commit();

                    // Breathe
                    context.DetachAll();

                    if (!attrs.HasNextPage)
                        break;
                }

                // Everything worked out, now delete all orpahned attributes
                if (attrIdsToDelete.Count > 0)
                {
                    try
                    {
                        // Don't rollback migration when this fails
                        var stubs = attrIdsToDelete.Select(x => new GenericAttribute { Id = x }).ToList();
                        foreach (var chunk in stubs.Slice(500))
                        {
                            chunk.Each(x => gaTable.Attach(x));
                            gaTable.RemoveRange(chunk);
                            scope.Commit();
                        }
                    }
                    catch (Exception ex)
                    {
                        var msg = ex.Message;
                    }
                }
            }

            return numUpdated;
        }


        #endregion

        #region CreateSystemMenus (V3.2)

        public static void CreateSystemMenus(IDbContext context)
        {
            var ctx = context as SmartObjectContext;
            if (ctx == null)
            {
                throw new ArgumentException("Passed context must be an instance of type '{0}'.".FormatInvariant(typeof(SmartObjectContext)), nameof(context));
            }

            const string entityProvider = "entity";
            const string routeProvider = "route";
            const string routeTemplate = "{{\"routename\":\"{0}\"}}";

            var resourceNames = new string[] {
                "Footer.Info",
                "Footer.Service",
                "Footer.Company",
                "Manufacturers.List",
                "Admin.Catalog.Categories",
                "Products.NewProducts",
                "Products.RecentlyViewedProducts",
                "Products.Compare.List",
                "ContactUs",
                "Blog",
                "Forum.Forums",
                "Account.Login",
                "Menu.ServiceMenu"
            };

            var settingNames = new string[]
            {
                "CatalogSettings.RecentlyAddedProductsEnabled",
                "CatalogSettings.RecentlyViewedProductsEnabled",
                "CatalogSettings.CompareProductsEnabled",
                "BlogSettings.Enabled",
                "ForumSettings.ForumsEnabled",
                "CustomerSettings.UserRegistrationType"
            };

            Dictionary<string, string> resources = null;
            Dictionary<string, string> settings = null;

            using (var scope = new DbContextScope(ctx: context, validateOnSave: false, hooksEnabled: false, autoCommit: false))
            {
                var permissionMigrator = new PermissionMigrator(ctx);
                permissionMigrator.AddPermission(new PermissionRecord
                {
                    Name = "Admin area. Manage Menus",
                    SystemName = "ManageMenus",
                    Category = "Content Management"
                }, new string[] { SystemCustomerRoleNames.Administrators });

                var menuSet = context.Set<MenuRecord>();
                var menuItemSet = context.Set<MenuItemRecord>();
                var defaultLang = context.Set<Language>().OrderBy(x => x.DisplayOrder).First();
                var manufacturerCount = context.Set<Manufacturer>().Count();
                var order = 0;

                resources = context.Set<LocaleStringResource>().AsNoTracking()
                    .Where(x => x.LanguageId == defaultLang.Id && resourceNames.Contains(x.ResourceName))
                    .Select(x => new { x.ResourceName, x.ResourceValue })
                    .ToList()
                    .ToDictionarySafe(x => x.ResourceName, x => x.ResourceValue, StringComparer.OrdinalIgnoreCase);

                settings = context.Set<Setting>().AsNoTracking()
                    .Where(x => x.StoreId == 0 && settingNames.Contains(x.Name))
                    .Select(x => new { x.Name, x.Value })
                    .ToList()
                    .ToDictionarySafe(x => x.Name, x => x.Value, StringComparer.OrdinalIgnoreCase);

                #region System menus

                var mainMenu = menuSet.Add(new MenuRecord
                {
                    SystemName = "Main",
                    IsSystemMenu = true,
                    Published = true,
                    Template = "Navbar",
                    Title = GetResource("Admin.Catalog.Categories")
                });

                var footerInfo = menuSet.Add(new MenuRecord
                {
                    SystemName = "FooterInformation",
                    IsSystemMenu = true,
                    Published = true,
                    Template = "LinkList",
                    Title = "Footer - " + GetResource("Footer.Info")
                });

                var footerService = menuSet.Add(new MenuRecord
                {
                    SystemName = "FooterService",
                    IsSystemMenu = true,
                    Published = true,
                    Template = "LinkList",
                    Title = "Footer - " + GetResource("Footer.Service")
                });

                var footerCompany = menuSet.Add(new MenuRecord
                {
                    SystemName = "FooterCompany",
                    IsSystemMenu = true,
                    Published = true,
                    Template = "LinkList",
                    Title = "Footer - " + GetResource("Footer.Company")
                });

                var serviceMenu = menuSet.Add(new MenuRecord
                {
                    SystemName = "HelpAndService",
                    IsSystemMenu = true,
                    Published = true,
                    Template = "Dropdown",
                    Title = GetResource("Menu.ServiceMenu").NullEmpty() ?? "Service"
                });

                scope.Commit();

                #endregion

                #region Main and footer menus

                menuItemSet.Add(new MenuItemRecord
                {
                    MenuId = mainMenu.Id,
                    ProviderName = "catalog",
                    Published = true
                });

                menuItemSet.Add(new MenuItemRecord
                {
                    MenuId = footerInfo.Id,
                    ProviderName = routeProvider,
                    Model = routeTemplate.FormatInvariant("ManufacturerList"),
                    Title = GetResource("Manufacturers.List"),
                    DisplayOrder = ++order,
                    Published = manufacturerCount > 0
                });
                menuItemSet.Add(new MenuItemRecord
                {
                    MenuId = footerInfo.Id,
                    ProviderName = routeProvider,
                    Model = routeTemplate.FormatInvariant("RecentlyAddedProducts"),
                    Title = GetResource("Products.NewProducts"),
                    DisplayOrder = ++order,
                    Published = GetSetting("CatalogSettings.RecentlyAddedProductsEnabled", true)
                });
                menuItemSet.Add(new MenuItemRecord
                {
                    MenuId = footerInfo.Id,
                    ProviderName = routeProvider,
                    Model = routeTemplate.FormatInvariant("RecentlyViewedProducts"),
                    Title = GetResource("Products.RecentlyViewedProducts"),
                    DisplayOrder = ++order,
                    Published = GetSetting("CatalogSettings.RecentlyViewedProductsEnabled", true)
                });
                menuItemSet.Add(new MenuItemRecord
                {
                    MenuId = footerInfo.Id,
                    ProviderName = routeProvider,
                    Model = routeTemplate.FormatInvariant("CompareProducts"),
                    Title = GetResource("Products.Compare.List"),
                    DisplayOrder = ++order,
                    Published = GetSetting("CatalogSettings.CompareProductsEnabled", true)
                });

                scope.Commit();
                order = 0;

                menuItemSet.Add(new MenuItemRecord
                {
                    MenuId = footerService.Id,
                    ProviderName = routeProvider,
                    Model = routeTemplate.FormatInvariant("contactus"),
                    Title = GetResource("ContactUs"),
                    DisplayOrder = ++order
                });
                menuItemSet.Add(new MenuItemRecord
                {
                    MenuId = footerService.Id,
                    ProviderName = routeProvider,
                    Model = routeTemplate.FormatInvariant("Blog"),
                    Title = GetResource("Blog"),
                    DisplayOrder = ++order,
                    Published = GetSetting("BlogSettings.Enabled", true)
                });
                menuItemSet.Add(new MenuItemRecord
                {
                    MenuId = footerService.Id,
                    ProviderName = routeProvider,
                    Model = routeTemplate.FormatInvariant("Boards"),
                    Title = GetResource("Forum.Forums"),
                    DisplayOrder = ++order,
                    Published = GetSetting("ForumSettings.ForumsEnabled", true)
                });
                menuItemSet.Add(new MenuItemRecord
                {
                    MenuId = footerService.Id,
                    ProviderName = entityProvider,
                    Model = "topic:shippinginfo",
                    DisplayOrder = ++order
                });
                menuItemSet.Add(new MenuItemRecord
                {
                    MenuId = footerService.Id,
                    ProviderName = entityProvider,
                    Model = "topic:paymentinfo",
                    DisplayOrder = ++order
                });

                scope.Commit();
                order = 0;

                menuItemSet.Add(new MenuItemRecord
                {
                    MenuId = footerCompany.Id,
                    ProviderName = entityProvider,
                    Model = "topic:aboutus",
                    DisplayOrder = ++order
                });
                menuItemSet.Add(new MenuItemRecord
                {
                    MenuId = footerCompany.Id,
                    ProviderName = entityProvider,
                    Model = "topic:imprint",
                    DisplayOrder = ++order
                });
                menuItemSet.Add(new MenuItemRecord
                {
                    MenuId = footerCompany.Id,
                    ProviderName = entityProvider,
                    Model = "topic:disclaimer",
                    DisplayOrder = ++order
                });
                menuItemSet.Add(new MenuItemRecord
                {
                    MenuId = footerCompany.Id,
                    ProviderName = entityProvider,
                    Model = "topic:privacyinfo",
                    DisplayOrder = ++order
                });
                menuItemSet.Add(new MenuItemRecord
                {
                    MenuId = footerCompany.Id,
                    ProviderName = entityProvider,
                    Model = "topic:conditionsofuse",
                    DisplayOrder = ++order
                });

                if (GetSetting("CustomerSettings.UserRegistrationType", "").IsCaseInsensitiveEqual("Disabled"))
                {
                    menuItemSet.Add(new MenuItemRecord
                    {
                        MenuId = footerCompany.Id,
                        ProviderName = routeProvider,
                        Model = routeTemplate.FormatInvariant("Login"),
                        Title = GetResource("Account.Login"),
                        DisplayOrder = ++order
                    });
                }

                scope.Commit();
                order = 0;

                #endregion

                #region Help & Service

                menuItemSet.Add(new MenuItemRecord
                {
                    MenuId = serviceMenu.Id,
                    ProviderName = routeProvider,
                    Model = routeTemplate.FormatInvariant("RecentlyAddedProducts"),
                    Title = GetResource("Products.NewProducts"),
                    DisplayOrder = ++order,
                    Published = GetSetting("CatalogSettings.RecentlyAddedProductsEnabled", true)
                });
                menuItemSet.Add(new MenuItemRecord
                {
                    MenuId = serviceMenu.Id,
                    ProviderName = routeProvider,
                    Model = routeTemplate.FormatInvariant("ManufacturerList"),
                    Title = GetResource("Manufacturers.List"),
                    DisplayOrder = ++order,
                    Published = manufacturerCount > 0
                });
                menuItemSet.Add(new MenuItemRecord
                {
                    MenuId = serviceMenu.Id,
                    ProviderName = routeProvider,
                    Model = routeTemplate.FormatInvariant("RecentlyViewedProducts"),
                    Title = GetResource("Products.RecentlyViewedProducts"),
                    DisplayOrder = ++order,
                    Published = GetSetting("CatalogSettings.RecentlyViewedProductsEnabled", true)
                });
                menuItemSet.Add(new MenuItemRecord
                {
                    MenuId = serviceMenu.Id,
                    ProviderName = routeProvider,
                    Model = routeTemplate.FormatInvariant("CompareProducts"),
                    Title = GetResource("Products.Compare.List"),
                    DisplayOrder = ++order,
                    Published = GetSetting("CatalogSettings.CompareProductsEnabled", true)
                });

                menuItemSet.Add(new MenuItemRecord
                {
                    MenuId = serviceMenu.Id,
                    ProviderName = entityProvider,
                    Model = "topic:aboutus",
                    DisplayOrder = ++order,
                    BeginGroup = true
                });
                menuItemSet.Add(new MenuItemRecord
                {
                    MenuId = serviceMenu.Id,
                    ProviderName = entityProvider,
                    Model = "topic:disclaimer",
                    DisplayOrder = ++order
                });
                menuItemSet.Add(new MenuItemRecord
                {
                    MenuId = serviceMenu.Id,
                    ProviderName = entityProvider,
                    Model = "topic:shippinginfo",
                    DisplayOrder = ++order
                });
                menuItemSet.Add(new MenuItemRecord
                {
                    MenuId = serviceMenu.Id,
                    ProviderName = entityProvider,
                    Model = "topic:conditionsofuse",
                    DisplayOrder = ++order
                });

                scope.Commit();
                order = 0;

                #endregion

                #region Localization

                var resourceSet = context.Set<LocaleStringResource>();

                var removeNames = new List<string> { "Menu.ServiceMenu" };
                var removeResources = resourceSet.Where(x => removeNames.Contains(x.ResourceName)).ToList();
                resourceSet.RemoveRange(removeResources);

                scope.Commit();

                #endregion
            }

            #region Utilities

            string GetResource(string name)
            {
                return resources.TryGetValue(name, out var value) ? value : string.Empty;
            }

            T GetSetting<T>(string name, T defaultValue = default(T))
            {
                try
                {
                    if (settings.TryGetValue(name, out var str) && CommonHelper.TryConvert(str, out T value))
                    {
                        return value;
                    }
                }
                catch { }

                return defaultValue;
            }

            #endregion
        }

        #endregion

        #region GranularPermissions (V4.0)

        public static void AddGranularPermissions(IDbContext context)
        {
            var ctx = context as SmartObjectContext;
            if (ctx == null)
            {
                throw new ArgumentException("Passed context must be an instance of type '{0}'.".FormatInvariant(typeof(SmartObjectContext)), nameof(context));
            }

            var permissionSet = ctx.Set<PermissionRecord>();
            var mappingSet = ctx.Set<PermissionRoleMapping>();
            var allRoles = ctx.Set<CustomerRole>().ToList();
            var allPermissions = permissionSet.Expand(x => x.CustomerRoles).ToList();
            var newPermissions = new Dictionary<string, PermissionRecord>();
            //var adminRole = allRoles.FirstOrDefault(x => x.SystemName.IsCaseInsensitiveEqual("Administrators"));

            var permissionToRoles = new Multimap<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var permission in allPermissions)
            {
                permissionToRoles.AddRange(permission.SystemName, permission.CustomerRoles.Select(x => x.Id));
            }

            using (var scope = new DbContextScope(ctx: context, validateOnSave: false, hooksEnabled: false, autoCommit: false))
            {
                var permissionSystemNames = PermissionHelper.GetPermissions(typeof(Permissions));

                // Delete existing granular permissions to ensure correct order in tree.
                permissionSet.RemoveRange(permissionSet.Where(x => permissionSystemNames.Contains(x.SystemName)).ToList());
                scope.Commit();

                // Insert new granular permissions.
                foreach (var permissionName in permissionSystemNames)
                {
                    var permission = permissionSet.Add(new PermissionRecord { SystemName = permissionName, Name = string.Empty, Category = string.Empty });
                    newPermissions[permissionName] = permission;
                }
                scope.Commit();

                // Migrate mappings of standard permissions (whether the new permission is granted).
                Allow("AccessAdminPanel", newPermissions[Permissions.System.AccessBackend]);
                Allow("AllowCustomerImpersonation", newPermissions[Permissions.Customer.Impersonate]);
                Allow("ManageCatalog", newPermissions[Permissions.Catalog.Self]);
                Allow("ManageCatalog", newPermissions[Permissions.Cart.CheckoutAttribute.Self]);
                Allow("ManageCustomers", newPermissions[Permissions.Customer.Self]);
                Allow("ManageCustomerRoles", newPermissions[Permissions.Customer.Role.Self]);
                Allow("ManageOrders", newPermissions[Permissions.Order.Self]);
                Allow("ManageGiftCards", newPermissions[Permissions.Order.GiftCard.Self]);
                Allow("ManageReturnRequests", newPermissions[Permissions.Order.ReturnRequest.Self]);
                Allow("ManageAffiliates", newPermissions[Permissions.Promotion.Affiliate.Self]);
                Allow("ManageCampaigns", newPermissions[Permissions.Promotion.Campaign.Self]);
                Allow("ManageDiscounts", newPermissions[Permissions.Promotion.Discount.Self]);
                Allow("ManageNewsletterSubscribers", newPermissions[Permissions.Promotion.Newsletter.Self]);
                Allow("ManagePolls", newPermissions[Permissions.Cms.Poll.Self]);
                Allow("ManageNews", newPermissions[Permissions.Cms.News.Self]);
                Allow("ManageBlog", newPermissions[Permissions.Cms.Blog.Self]);
                Allow("ManageWidgets", newPermissions[Permissions.Cms.Widget.Self]);
                Allow("ManageTopics", newPermissions[Permissions.Cms.Topic.Self]);
                Allow("ManageMenus", newPermissions[Permissions.Cms.Menu.Self]);
                Allow("ManageForums", newPermissions[Permissions.Cms.Forum.Self]);
                Allow("ManageMessageTemplates", newPermissions[Permissions.Cms.MessageTemplate.Self]);
                Allow("ManageCountries", newPermissions[Permissions.Configuration.Country.Self]);
                Allow("ManageLanguages", newPermissions[Permissions.Configuration.Language.Self]);
                Allow("ManageSettings", newPermissions[Permissions.Configuration.Setting.Self]);
                Allow("ManagePaymentMethods", newPermissions[Permissions.Configuration.PaymentMethod.Self]);
                Allow("ManageExternalAuthenticationMethods", newPermissions[Permissions.Configuration.Authentication.Self]);
                Allow("ManageTaxSettings", newPermissions[Permissions.Configuration.Tax.Self]);
                Allow("ManageShippingSettings", newPermissions[Permissions.Configuration.Shipping.Self]);
                Allow("ManageCurrencies", newPermissions[Permissions.Configuration.Currency.Self]);
                Allow("ManageDeliveryTimes", newPermissions[Permissions.Configuration.DeliveryTime.Self]);
                Allow("ManageThemes", newPermissions[Permissions.Configuration.Theme.Self]);
                Allow("ManageMeasures", newPermissions[Permissions.Configuration.Measure.Self]);
                Allow("ManageActivityLog", newPermissions[Permissions.Configuration.ActivityLog.Self]);
                Allow("ManageACL", newPermissions[Permissions.Configuration.Acl.Self]);
                Allow("ManageEmailAccounts", newPermissions[Permissions.Configuration.EmailAccount.Self]);
                Allow("ManageStores", newPermissions[Permissions.Configuration.Store.Self]);
                Allow("ManagePlugins", newPermissions[Permissions.Configuration.Plugin.Self]);
                Allow("ManageSystemLog", newPermissions[Permissions.System.Log.Self]);
                Allow("ManageMessageQueue", newPermissions[Permissions.System.Message.Self]);
                Allow("ManageMaintenance", newPermissions[Permissions.System.Maintenance.Self]);
                Allow("UploadPictures", newPermissions[Permissions.Media.Self]);
                Allow("ManageScheduleTasks", newPermissions[Permissions.System.ScheduleTask.Self]);
                Allow("ManageExports", newPermissions[Permissions.Configuration.Export.Self]);
                Allow("ManageImports", newPermissions[Permissions.Configuration.Import.Self]);
                Allow("ManageUrlRecords", newPermissions[Permissions.System.UrlRecord.Self]);
                Allow("DisplayPrices", newPermissions[Permissions.Catalog.DisplayPrice]);
                Allow("EnableShoppingCart", newPermissions[Permissions.Cart.AccessShoppingCart]);
                Allow("EnableWishlist", newPermissions[Permissions.Cart.AccessWishlist]);
                Allow("PublicStoreAllowNavigation", newPermissions[Permissions.System.AccessShop]);

                // Add mappings for new permissions.
                //AllowForRole(adminRole, newPermissions[Permissions.....]);

                scope.Commit();
                newPermissions.Clear();

                // Migrate known plugin permissions.
                var pluginPermissionNames = new Dictionary<string, string>
                {
                    { "ManageDebitoor", "SmartStore.Debitoor.Security.DebitoorPermissions, SmartStore.Debitoor" },
                    { "AccessImportBiz", "SmartStore.BizImporter.Security.BizImporterPermissions, SmartStore.BizImporter" },
                    { "ManageShopConnector", "SmartStore.ShopConnector.Security.ShopConnectorPermissions, SmartStore.ShopConnector" },
                    { "ManageNewsImporter", "SmartStore.NewsImporter.Security.NewImporterPermissions, SmartStore.NewsImporter" },
                    { "ManageWebApi", "SmartStore.WebApi.Security.WebApiPermissions, SmartStore.WebApi" },
                    { "ManageMegaSearch", "SmartStore.MegaSearch.Security.MegaSearchPermissions, SmartStore.MegaSearch" },
                    { "ManageErpConnector", "Srt.ErpConnector.Security.ErpConnectorPermissions, Srt.ErpConnector" },
                    { "ManagePowerBi", "SmartStore.PowerBi.Security.PowerBiPermissions, SmartStore.PowerBi" },
                    { "ManageWallet", "SmartStore.Wallet.Security.WalletPermissions, SmartStore.Wallet" },
                    { "ManageStories", "SmartStore.PageBuilder.Services.PageBuilderPermissions, SmartStore.PageBuilder" },
                    { "ManageDlm", "SmartStore.Dlm.Security.DlmPermissions, SmartStore.Dlm" }
                };

                var allPluginPermissionNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var kvp in pluginPermissionNames)
                {
                    if (permissionToRoles.ContainsKey(kvp.Key))
                    {
                        var assemblyName = Type.GetType(kvp.Value)?.AssemblyQualifiedName;
                        if (assemblyName.HasValue())
                        {
                            var type = Type.GetType(assemblyName);
                            allPluginPermissionNames.AddRange(PermissionHelper.GetPermissions(type));
                        }
                        else
                        {
                            $"Plugin permission type not found ({kvp.Value}).".Dump();
                        }
                    }
                    else
                    {
                        // Ignore unknown permissions. Will be deleted later by another migration.
                    }
                }

                // Delete existing granular permissions to ensure correct order in tree.
                permissionSet.RemoveRange(permissionSet.Where(x => allPluginPermissionNames.Contains(x.SystemName)).ToList());
                scope.Commit();

                // Insert new granular permissions.
                foreach (var permissionName in allPluginPermissionNames)
                {
                    var permission = permissionSet.Add(new PermissionRecord { SystemName = permissionName, Name = string.Empty, Category = string.Empty });
                    newPermissions[permissionName] = permission;
                }
                scope.Commit();

                PermissionRecord pr;
                if (newPermissions.TryGetValue("debitoor", out pr)) Allow("ManageDebitoor", pr);
                if (newPermissions.TryGetValue("biz-importer", out pr)) Allow("AccessImportBiz", pr);
                if (newPermissions.TryGetValue("shop-connector", out pr)) Allow("ManageShopConnector", pr);
                if (newPermissions.TryGetValue("news-importer", out pr)) Allow("ManageNewsImporter", pr);
                if (newPermissions.TryGetValue("web-api", out pr)) Allow("ManageWebApi", pr);
                if (newPermissions.TryGetValue("megasearch", out pr)) Allow("ManageMegaSearch", pr);
                if (newPermissions.TryGetValue("erp-connector", out pr)) Allow("ManageErpConnector", pr);
                if (newPermissions.TryGetValue("power-bi", out pr)) Allow("ManagePowerBi", pr);
                if (newPermissions.TryGetValue("wallet", out pr)) Allow("ManageWallet", pr);
                if (newPermissions.TryGetValue("page-builder", out pr)) Allow("ManageStories", pr);
                if (newPermissions.TryGetValue("dlm", out pr)) Allow("ManageDlm", pr);

                scope.Commit();
            }

            void Allow(string oldSystemName, params PermissionRecord[] permissions)
            {
                var appliedRoleIds = permissionToRoles.ContainsKey(oldSystemName)
                    ? permissionToRoles[oldSystemName]
                    : Enumerable.Empty<int>();

                foreach (var permission in permissions)
                {
                    Guard.NotZero(permission.Id, nameof(permission.Id));

                    foreach (var roleId in appliedRoleIds)
                    {
                        mappingSet.Add(new PermissionRoleMapping
                        {
                            Allow = true,
                            PermissionRecordId = permission.Id,
                            CustomerRoleId = roleId
                        });
                    }
                }
            }

            //void AllowForRole(CustomerRole role, params PermissionRecord[] permissions)
            //{
            //    foreach (var permission in permissions)
            //    {
            //        mappingSet.Add(new PermissionRoleMapping
            //        {
            //            Allow = true,
            //            PermissionRecordId = permission.Id,
            //            CustomerRoleId = role.Id
            //        });
            //    }
            //}
        }

        #endregion
    }
}
