using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.Serialization;
using Newtonsoft.Json;
using SmartStore.Collections;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Cms;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Configuration;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Discounts;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Domain.Payments;
using SmartStore.Core.Domain.Security;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.IO;
using SmartStore.Core.Logging;
using SmartStore.Core.Security;
using SmartStore.Rules;
using SmartStore.Rules.Domain;
using SmartStore.Utilities;
using SmartStore.Utilities.ObjectPools;
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
        public static bool FixProductMainPictureId(IDbContext context, Product product, IEnumerable<ProductMediaFile> entities = null)
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

            var newMainPictureId = sortedEntities.FirstOrDefault()?.MediaFileId;

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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

            var query = from pp in context.Set<ProductMediaFile>().AsNoTracking()
                        where productIds.Contains(pp.ProductId)
                        group pp by pp.ProductId into g
                        select new
                        {
                            ProductId = g.Key,
                            PictureIds = g.OrderBy(x => x.DisplayOrder).ThenBy(x => x.Id)
                                .Take(1)
                                .Select(x => x.MediaFileId)
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
                            fs.CreateFolder(subfolder);
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

        class CustomerStub
        {
            public int Id { get; set; }
            public IEnumerable<AttributeStub> Attributes { get; set; }
        }

        class AttributeStub
        {
            public int Id { get; set; }
            public string Key { get; set; }
            public string Value { get; set; }
        }

        /// <summary>
        /// Moves several customer fields saved as generic attributes to customer entity (Title, FirstName, LastName, BirthDate, Company, CustomerNumber)
        /// </summary>
        /// <param name="context">Database context (must be <see cref="SmartObjectContext"/>)</param>
        /// <returns>The total count of fixed and updated customer entities</returns>
        public static int MoveCustomerFields(
            IDbContext context,
            Action<IDictionary<string, object>, string, string> updater,
            params string[] candidates)
        {
            Guard.NotNull(updater, nameof(updater));

            if (!(context is SmartObjectContext ctx))
                throw new ArgumentException("Passed context must be an instance of type '{0}'.".FormatInvariant(typeof(SmartObjectContext)), nameof(context));

            if (candidates.Length == 0)
                return 0;

            const int pageSize = 1000;
            var gaTable = context.Set<GenericAttribute>().AsNoTracking();
            //var candidates = new[] { "Title", "FirstName", "LastName", "Company", "CustomerNumber", "DateOfBirth" };

            var query = (
                from a in gaTable
                where a.KeyGroup == "Customer" && candidates.Contains(a.Key)
                group a by a.EntityId into grps
                where grps.Any()
                select new CustomerStub
                {
                    Id = grps.Key,
                    Attributes = grps.Select(a2 => new AttributeStub { Id = a2.Id, Key = a2.Key, Value = a2.Value })
                })
                .OrderBy(x => x.Id);

            int numAffectedRows = 0;
            int numUpdated = 0;

            using (var scope = new DbContextScope(ctx: context, validateOnSave: false, hooksEnabled: false, autoCommit: false))
            {
                for (var pageIndex = 0; pageIndex < 9999999; pageIndex++)
                {
                    var data = query
                        .Skip(pageIndex * pageSize)
                        .Take(pageSize)
                        .ToList();

                    foreach (var chunk in data.Slice(100))
                    {
                        numAffectedRows += GenerateAndExecuteSql(chunk);
                    }

                    numUpdated += data.Count;

                    if (data.Count == 0)
                        break;
                }
            }

            return numUpdated;

            int GenerateAndExecuteSql(IEnumerable<CustomerStub> customers)
            {
                var sb = PooledStringBuilder.Rent();

                foreach (var c in customers)
                {
                    var attrs = c.Attributes.ToArray();

                    if (attrs.Length == 0)
                        continue;

                    var columns = new Dictionary<string, object>();

                    /// Generate UPDATE Customer statement
                    foreach (var attr in attrs)
                    {
                        updater(columns, attr.Key, attr.Value);
                    }

                    if (columns.Count == 0)
                        continue;

                    sb.Append("UPDATE [Customer] SET");

                    int i = 0;
                    foreach (var kvp in columns)
                    {
                        sb.Append(" ");
                        if (i > 0) sb.Append(", ");

                        sb.Append($"[{kvp.Key}] = ");

                        if (kvp.Value is int n)
                        {
                            sb.Append($"{n.ToString(CultureInfo.InvariantCulture)}");
                        }
                        else if (kvp.Value is DateTime d)
                        {
                            sb.Append($"'{d.ToIso8601String()}'");
                        }
                        else if (kvp.Value == null)
                        {
                            sb.Append("NULL");
                        }
                        else
                        {
                            sb.Append($"N'{kvp.Value}'");
                        }

                        i++;
                    }

                    sb.Append(" WHERE Id = {0}".FormatInvariant(c.Id));
                    sb.Append("\n");

                    // Generate all GenericAttribute DELETE statements
                    for (i = 0; i < attrs.Length; i++)
                    {
                        var a = attrs[i];
                        sb.AppendLine($"DELETE FROM [GenericAttribute] WHERE [Id] = {a.Id}");
                    }
                }

                var sql = sb.ToStringAndReturn();
                if (sql.HasValue())
                {
                    return ctx.Execute(sql);
                }

                return 0;
            }
        }

        public static int DeleteGuestCustomerGenericAttributes(IDbContext context, TimeSpan olderThan)
        {
            if (!(context is SmartObjectContext ctx))
            {
                throw new ArgumentException("Passed context must be an instance of type '{0}'.".FormatInvariant(typeof(SmartObjectContext)), nameof(context));
            }

            int numTotalDeleted = 0;
            var maxDate = DateTime.UtcNow - olderThan;

            var sql = @"
DELETE TOP(50000) [g]
  FROM [dbo].[GenericAttribute] AS [g]
  LEFT OUTER JOIN [dbo].[Customer] AS [c] ON c.Id = g.EntityId
  LEFT OUTER JOIN [dbo].[Order] AS [o] ON c.Id = o.CustomerId
  LEFT OUTER JOIN [dbo].[CustomerContent] AS [cc] ON c.Id = cc.CustomerId
  LEFT OUTER JOIN [dbo].[Forums_PrivateMessage] AS [pm] ON c.Id = pm.ToCustomerId
  LEFT OUTER JOIN [dbo].[Forums_Post] AS [fp] ON c.Id = fp.CustomerId
  LEFT OUTER JOIN [dbo].[Forums_Topic] AS [ft] ON c.Id = ft.CustomerId
  WHERE g.KeyGroup = 'Customer' AND c.Username IS Null AND c.Email IS NULL AND c.IsSystemAccount = 0 AND c.LastActivityDateUtc < @p0
	AND (NOT EXISTS (SELECT 1 AS [C1] FROM [dbo].[ShoppingCartItem] AS [sci] WHERE c.Id = sci.CustomerId ))  
	AND (NOT EXISTS (SELECT 1 AS [C1] FROM [dbo].[Order] AS [o1] WHERE c.Id = o1.CustomerId ))
	AND (NOT EXISTS (SELECT 1 AS [C1] FROM [dbo].[CustomerContent] AS [cc1] WHERE c.Id = cc1.CustomerId ))
	AND (NOT EXISTS (SELECT 1 AS [C1] FROM [dbo].[Forums_PrivateMessage] AS [pm1] WHERE c.Id = pm1.ToCustomerId ))
	AND (NOT EXISTS (SELECT 1 AS [C1] FROM [dbo].[Forums_Post] AS [fp1] WHERE c.Id = fp1.CustomerId ))
	AND (NOT EXISTS (SELECT 1 AS [C1] FROM [dbo].[Forums_Topic] AS [ft1] WHERE c.Id = ft1.CustomerId ))
";

            while (true)
            {
                var numDeleted = ctx.Execute(sql, maxDate);
                if (numDeleted == 0)
                    break;

                numTotalDeleted += numDeleted;
            }

            return numTotalDeleted;
        }

        public static int DeleteGuestCustomers(IDbContext context, TimeSpan olderThan)
        {
            if (!(context is SmartObjectContext ctx))
            {
                throw new ArgumentException("Passed context must be an instance of type '{0}'.".FormatInvariant(typeof(SmartObjectContext)), nameof(context));
            }

            int numTotalDeleted = 0;
            var maxDate = DateTime.UtcNow - olderThan;

            var sql = @"
DELETE TOP(20000) [c]
  FROM [dbo].[Customer] AS [c]
  LEFT OUTER JOIN [dbo].[Order] AS [o] ON c.Id = o.CustomerId
  LEFT OUTER JOIN [dbo].[CustomerContent] AS [cc] ON c.Id = cc.CustomerId
  LEFT OUTER JOIN [dbo].[Forums_PrivateMessage] AS [pm] ON c.Id = pm.ToCustomerId
  LEFT OUTER JOIN [dbo].[Forums_Post] AS [fp] ON c.Id = fp.CustomerId
  LEFT OUTER JOIN [dbo].[Forums_Topic] AS [ft] ON c.Id = ft.CustomerId
  WHERE c.Username IS Null AND c.Email IS NULL AND c.IsSystemAccount = 0 AND c.LastActivityDateUtc < @p0
	AND (NOT EXISTS (SELECT 1 AS x FROM [dbo].[ShoppingCartItem] AS [sci] WHERE c.Id = sci.CustomerId ))  
	AND (NOT EXISTS (SELECT 1 AS x FROM [dbo].[Order] AS [o1] WHERE c.Id = o1.CustomerId ))
	AND (NOT EXISTS (SELECT 1 AS x FROM [dbo].[CustomerContent] AS [cc1] WHERE c.Id = cc1.CustomerId ))
	AND (NOT EXISTS (SELECT 1 AS x FROM [dbo].[Forums_PrivateMessage] AS [pm1] WHERE c.Id = pm1.ToCustomerId ))
	AND (NOT EXISTS (SELECT 1 AS x FROM [dbo].[Forums_Post] AS [fp1] WHERE c.Id = fp1.CustomerId ))
	AND (NOT EXISTS (SELECT 1 AS x FROM [dbo].[Forums_Topic] AS [ft1] WHERE c.Id = ft1.CustomerId ))
";

            while (true)
            {
                var numDeleted = ctx.Execute(sql, maxDate);
                if (numDeleted == 0)
                    break;

                numTotalDeleted += numDeleted;
            }

            return numTotalDeleted;
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
                menuItemSet.Add(new MenuItemRecord
                {
                    MenuId = footerService.Id,
                    ProviderName = routeProvider,
                    Model = routeTemplate.FormatInvariant("CookieManager"),
                    Title = "Cookie Manager",
                    DisplayOrder = ++order,
                    CssClass = "cookie-manager"
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

            var mappingSet = ctx.Set<PermissionRoleMapping>();
            var oldPermissions = GetOldPermissions();

            var allRoleIds = ctx.Set<CustomerRole>()
                .AsNoTracking()
                .Where(x => !string.IsNullOrEmpty(x.SystemName))
                .Select(x => new { x.Id, x.SystemName })
                .ToList()
                .ToDictionarySafe(x => x.SystemName, x => x.Id, StringComparer.OrdinalIgnoreCase);

            allRoleIds.TryGetValue(SystemCustomerRoleNames.Administrators, out var adminRoleId);
            allRoleIds.TryGetValue(SystemCustomerRoleNames.ForumModerators, out var forumModRoleId);
            allRoleIds.TryGetValue(SystemCustomerRoleNames.Registered, out var registeredRoleId);
            allRoleIds.TryGetValue(SystemCustomerRoleNames.Guests, out var guestRoleId);
            allRoleIds.Clear();

            // Mapping has no entity and no navigation property -> use SQL.
            var oldMappings = context.SqlQuery<OldPermissionRoleMapping>("select * from [dbo].[PermissionRecord_Role_Mapping]")
                .ToList()
                .ToMultimap(x => x.PermissionRecord_Id, x => x.CustomerRole_Id);

            var permissionToRoles = new Multimap<string, int>(StringComparer.OrdinalIgnoreCase);
            var allPermissions = ctx.Set<PermissionRecord>()
                .AsNoTracking()
                .Select(x => new { x.Id, x.SystemName })
                .ToList();

            foreach (var permission in allPermissions)
            {
                var roleIds = oldMappings.ContainsKey(permission.Id)
                    ? oldMappings[permission.Id]
                    : Enumerable.Empty<int>();

                permissionToRoles.AddRange(permission.SystemName, roleIds);
            }
            allPermissions.Clear();

            using (var scope = new DbContextScope(ctx: context, validateOnSave: false, hooksEnabled: false, autoCommit: false))
            {
                var permissionSystemNames = PermissionHelper.GetPermissions(typeof(Permissions));

                var newPermissions = InsertPermissions(scope, permissionSystemNames);

                // Migrate mappings of standard permissions (whether the new permission is granted).
                foreach (var kvp in oldPermissions)
                {
                    foreach (var name in kvp.Value)
                    {
                        Allow(kvp.Key, newPermissions[name]);
                    }
                }
                // Commit to avoid duplicate mappings!
                scope.Commit();

                // Add mappings for new permissions.
                AllowForRole(adminRoleId,
                    newPermissions[Permissions.Cart.Read],
                    newPermissions[Permissions.System.Rule.Self]);

                // Add mappings originally added by old migrations.
                // We had to remove these migration statements again because the table does not yet exist at this time.
                AllowForRole(adminRoleId,
                    newPermissions[Permissions.Configuration.Export.Self],
                    newPermissions[Permissions.Configuration.Import.Self],
                    newPermissions[Permissions.System.UrlRecord.Self],
                    newPermissions[Permissions.Cms.Menu.Self]);

                scope.Commit();

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

                // Get new plugin permission names.
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

                newPermissions.Clear();
                newPermissions = InsertPermissions(scope, allPluginPermissionNames);

                // Add PermissionRoleMapping for old plugin permissions.
                PermissionRecord pr;
                if (newPermissions.TryGetValue("debitoor", out pr))
                {
                    Allow("ManageDebitoor", pr);
                }
                if (newPermissions.TryGetValue("bizimporter", out pr))
                {
                    Allow("AccessImportBiz", pr);
                }
                if (newPermissions.TryGetValue("shopconnector", out pr))
                {
                    Allow("ManageShopConnector", pr);
                }
                if (newPermissions.TryGetValue("newsimporter", out pr))
                {
                    Allow("ManageNewsImporter", pr);
                }
                if (newPermissions.TryGetValue("webapi", out pr))
                {
                    Allow("ManageWebApi", pr);
                }
                if (newPermissions.TryGetValue("megasearch", out pr))
                {
                    Allow("ManageMegaSearch", pr);
                }
                if (newPermissions.TryGetValue("erpconnector", out pr))
                {
                    Allow("ManageErpConnector", pr);
                }
                if (newPermissions.TryGetValue("powerbi", out pr))
                {
                    Allow("ManagePowerBi", pr);
                }
                if (newPermissions.TryGetValue("wallet", out pr))
                {
                    Allow("ManageWallet", pr);
                }
                if (newPermissions.TryGetValue("pagebuilder", out pr))
                {
                    Allow("ManageStories", pr);
                }
                if (newPermissions.TryGetValue("dlm", out pr))
                {
                    Allow("ManageDlm", pr);
                }

                // Add PermissionRoleMapping for default plugin permissions.
                if (newPermissions.TryGetValue("pagebuilder.displaystory", out pr))
                {
                    AllowForRole(forumModRoleId, pr);
                    AllowForRole(guestRoleId, pr);
                    AllowForRole(registeredRoleId, pr);
                }
                if (newPermissions.TryGetValue("contentslider.displayslider", out pr))
                {
                    AllowForRole(forumModRoleId, pr);
                    AllowForRole(guestRoleId, pr);
                    AllowForRole(registeredRoleId, pr);
                }

                scope.Commit();

                // Migrate permission names of menu items.
                var menuItems = ctx.Set<MenuItemRecord>().Where(x => !string.IsNullOrEmpty(x.PermissionNames)).ToList();
                if (menuItems.Any())
                {
                    foreach (var item in menuItems)
                    {
                        var newNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                        var names = item.PermissionNames.SplitSafe(",");
                        foreach (var name in names)
                        {
                            if (oldPermissions.ContainsKey(name))
                            {
                                oldPermissions[name].Each(x => newNames.Add(x));
                            }
                        }
                        item.PermissionNames = string.Join(",", newNames);
                    }

                    scope.Commit();
                }
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

            void AllowForRole(int roleId, params PermissionRecord[] permissions)
            {
                foreach (var permission in permissions)
                {
                    if (!mappingSet.Any(x => x.PermissionRecordId == permission.Id && x.CustomerRoleId == roleId))
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

            Multimap<string, string> GetOldPermissions()
            {
                var map = new Multimap<string, string>(StringComparer.OrdinalIgnoreCase);
                map.Add("AccessAdminPanel", Permissions.System.AccessBackend);
                map.Add("AllowCustomerImpersonation", Permissions.Customer.Impersonate);
                map.AddRange("ManageCatalog", new string[] { Permissions.Catalog.Self, Permissions.Cart.CheckoutAttribute.Self });
                map.Add("ManageCustomers", Permissions.Customer.Self);
                map.Add("ManageCustomerRoles", Permissions.Customer.Role.Self);
                map.Add("ManageOrders", Permissions.Order.Self);
                map.Add("ManageGiftCards", Permissions.Order.GiftCard.Self);
                map.Add("ManageReturnRequests", Permissions.Order.ReturnRequest.Self);
                map.Add("ManageAffiliates", Permissions.Promotion.Affiliate.Self);
                map.Add("ManageCampaigns", Permissions.Promotion.Campaign.Self);
                map.Add("ManageDiscounts", Permissions.Promotion.Discount.Self);
                map.Add("ManageNewsletterSubscribers", Permissions.Promotion.Newsletter.Self);
                map.Add("ManagePolls", Permissions.Cms.Poll.Self);
                map.Add("ManageNews", Permissions.Cms.News.Self);
                map.Add("ManageBlog", Permissions.Cms.Blog.Self);
                map.Add("ManageWidgets", Permissions.Cms.Widget.Self);
                map.Add("ManageTopics", Permissions.Cms.Topic.Self);
                map.Add("ManageMenus", Permissions.Cms.Menu.Self);
                map.Add("ManageForums", Permissions.Cms.Forum.Self);
                map.Add("ManageMessageTemplates", Permissions.Cms.MessageTemplate.Self);
                map.Add("ManageCountries", Permissions.Configuration.Country.Self);
                map.Add("ManageLanguages", Permissions.Configuration.Language.Self);
                map.Add("ManageSettings", Permissions.Configuration.Setting.Self);
                map.Add("ManagePaymentMethods", Permissions.Configuration.PaymentMethod.Self);
                map.Add("ManageExternalAuthenticationMethods", Permissions.Configuration.Authentication.Self);
                map.Add("ManageTaxSettings", Permissions.Configuration.Tax.Self);
                map.Add("ManageShippingSettings", Permissions.Configuration.Shipping.Self);
                map.Add("ManageCurrencies", Permissions.Configuration.Currency.Self);
                map.Add("ManageDeliveryTimes", Permissions.Configuration.DeliveryTime.Self);
                map.Add("ManageThemes", Permissions.Configuration.Theme.Self);
                map.Add("ManageMeasures", Permissions.Configuration.Measure.Self);
                map.Add("ManageActivityLog", Permissions.Configuration.ActivityLog.Self);
                map.Add("ManageACL", Permissions.Configuration.Acl.Self);
                map.Add("ManageEmailAccounts", Permissions.Configuration.EmailAccount.Self);
                map.Add("ManageStores", Permissions.Configuration.Store.Self);
                map.Add("ManagePlugins", Permissions.Configuration.Plugin.Self);
                map.Add("ManageSystemLog", Permissions.System.Log.Self);
                map.Add("ManageMessageQueue", Permissions.System.Message.Self);
                map.Add("ManageMaintenance", Permissions.System.Maintenance.Self);
                map.Add("UploadPictures", Permissions.Media.Self);
                map.Add("ManageScheduleTasks", Permissions.System.ScheduleTask.Self);
                map.Add("ManageExports", Permissions.Configuration.Export.Self);
                map.Add("ManageImports", Permissions.Configuration.Import.Self);
                map.Add("ManageUrlRecords", Permissions.System.UrlRecord.Self);
                map.Add("DisplayPrices", Permissions.Catalog.DisplayPrice);
                map.Add("EnableShoppingCart", Permissions.Cart.AccessShoppingCart);
                map.Add("EnableWishlist", Permissions.Cart.AccessWishlist);
                map.Add("PublicStoreAllowNavigation", Permissions.System.AccessShop);

                return map;
            }

            Dictionary<string, PermissionRecord> InsertPermissions(DbContextScope scope, IEnumerable<string> systemNames)
            {
                var newPermissions = new Dictionary<string, PermissionRecord>();
                var newPermissionSql = "Select * From [dbo].[PermissionRecord] Where [Name] = '' And [Category] = ''";

                // Delete existing granular permissions to ensure correct order in tree.
                // Maybe this step is not required anymore but doesn't hurt.
                var existingPermissions = ctx.SqlQuery<PermissionRecord>(newPermissionSql);
                foreach (var permission in existingPermissions)
                {
                    if (systemNames.Contains(permission.SystemName))
                    {
                        ctx.Database.ExecuteSqlCommand("Delete From [dbo].[PermissionRecord] Where [Id] = {0}", permission.Id);
                    }
                }

                // Insert new granular permissions.
                // Name and category property cannot be null, exist in database, but not in the domain model -> use SQL.
                foreach (var name in systemNames)
                {
                    ctx.Database.ExecuteSqlCommand("Insert Into PermissionRecord (SystemName, Name, Category) Values({0}, {1}, {2})",
                        name, string.Empty, string.Empty);
                }

                // Now load new permissions.
                var permissions = ctx.SqlQuery<PermissionRecord>(newPermissionSql);
                foreach (var permission in permissions)
                {
                    if (systemNames.Contains(permission.SystemName))
                    {
                        // It's a new granular permission.
                        newPermissions[permission.SystemName] = permission;
                    }
                }

                return newPermissions;
            }
        }

        public class OldPermissionRoleMapping
        {
            public int PermissionRecord_Id { get; set; }
            public int CustomerRole_Id { get; set; }
        }

        public static void AddRuleSets(IDbContext context)
        {
            var ctx = context as SmartObjectContext;
            if (ctx == null)
            {
                throw new ArgumentException("Passed context must be an instance of type '{0}'.".FormatInvariant(typeof(SmartObjectContext)), nameof(context));
            }

            var defaultLang = ctx.Set<Language>().AsNoTracking().OrderBy(x => x.DisplayOrder).First();
            var isGerman = defaultLang.UniqueSeoCode.IsCaseInsensitiveEqual("de");
            var discountNameTemplate = isGerman ? "Regel für Rabatt \"{0}\"" : "Rule for discount \"{0}\"";
            var shippingNameTemplate = isGerman ? "Regel für Versandart \"{0}\"" : "Rule for shipping method \"{0}\"";
            var paymentNameTemplate = isGerman ? "Regel für Zahlart \"{0} ({1})\"" : "Rule for payment method \"{0} ({1})\"";
            var utcNow = DateTime.UtcNow;

            var cartRules = ctx.Set<RuleSetEntity>()
                .Include(x => x.Rules)
                .Where(x => x.Scope == RuleScope.Cart && !x.IsSubGroup)
                .ToList();

            var existingRuleSets = cartRules
                .Where(x => x.Name.HasValue())
                .ToDictionarySafe(x => x.Name, x => x, StringComparer.OrdinalIgnoreCase);

            using (var scope = new DbContextScope(ctx: context, validateOnSave: false, hooksEnabled: false, autoCommit: false))
            {
                AddRuleSetsForDiscountRequirements();
                scope.Commit();

                AddRuleSetsForShippingFilters();
                scope.Commit();

                AddRuleSetsForPaymentFilters();
                scope.Commit();
            }

            void AddRuleSetsForDiscountRequirements()
            {
                var allRequirements = ctx.SqlQuery<OldDiscountRequirement>("select * from [dbo].[DiscountRequirement]")
                    .ToList()
                    .ToMultimap(x => x.DiscountId, x => x);

                if (!allRequirements.Any())
                {
                    return;
                }

                var discountIds = allRequirements.Select(x => x.Key).ToList();
                var discounts = ctx.Set<Discount>()
                    .Where(x => discountIds.Contains(x.Id))
                    .ToList();

                if (!discounts.Any())
                {
                    return;
                }

                foreach (var discount in discounts)
                {
                    var ruleSetName = discountNameTemplate.FormatInvariant(discount.Name.NullEmpty() ?? discount.Id.ToString()).Truncate(195, "…");

                    // Avoid adding rule sets multiple times.
                    if (!existingRuleSets.TryGetValue(ruleSetName, out var ruleSet))
                    {
                        // All requirements must be fulfilled for a discount to be applied.
                        ruleSet = new RuleSetEntity
                        {
                            Name = ruleSetName,
                            IsActive = true,
                            Scope = RuleScope.Cart,
                            LogicalOperator = LogicalRuleOperator.And,
                            CreatedOnUtc = utcNow,
                            UpdatedOnUtc = utcNow
                        };

                        // Try to combine requirements to one single rule, if possible.
                        var requirements = allRequirements[discount.Id]
                            .ToMultimap(x => x.DiscountRequirementRuleSystemName.EmptyNull().ToLower(), x => x, StringComparer.OrdinalIgnoreCase);

                        foreach (var name in requirements.Keys)
                        {
                            switch (name)
                            {
                                case "discountrequirement.billingcountryis":
                                    foreach (var requirement in requirements[name].Where(x => x.BillingCountryId != 0))
                                    {
                                        ruleSet.Rules.Add(new RuleEntity
                                        {
                                            RuleType = "CartBillingCountry",
                                            Operator = RuleOperator.In,
                                            Value = requirement.BillingCountryId.ToString()
                                        });
                                    }
                                    break;
                                case "discountrequirement.shippingcountryis":
                                    foreach (var requirement in requirements[name].Where(x => x.ShippingCountryId != 0))
                                    {
                                        ruleSet.Rules.Add(new RuleEntity
                                        {
                                            RuleType = "CartShippingCountry",
                                            Operator = RuleOperator.In,
                                            Value = requirement.ShippingCountryId.ToString()
                                        });
                                    }
                                    break;
                                case "discountrequirement.mustbeassignedtocustomerrole":
                                    var roleIds = new HashSet<int>(requirements[name]
                                        .Select(x => x.RestrictedToCustomerRoleId ?? 0)
                                        .Where(x => x != 0));

                                    if (roleIds.Any())
                                    {
                                        ruleSet.Rules.Add(new RuleEntity
                                        {
                                            RuleType = "CustomerRole",
                                            Operator = RuleOperator.Contains,
                                            Value = string.Join(",", roleIds)
                                        });
                                    }
                                    break;
                                case "discountrequirement.hadspentamount":
                                    foreach (var requirement in requirements[name].Where(x => x.SpentAmount != decimal.Zero))
                                    {
                                        var limitToCurrentBasketSubTotal = GetExtraData<bool>(requirement, "LimitToCurrentBasketSubTotal");

                                        ruleSet.Rules.Add(new RuleEntity
                                        {
                                            RuleType = limitToCurrentBasketSubTotal ? "CartSubtotal" : "CartSpentAmount",
                                            Operator = RuleOperator.GreaterThanOrEqualTo,
                                            Value = requirement.SpentAmount.ToString(CultureInfo.InvariantCulture)
                                        });
                                    }
                                    break;
                                case "discountrequirement.hasallproducts":
                                case "discountrequirement.hasoneproduct":
                                case "discountrequirement.purchasedallproducts":
                                case "discountrequirement.purchasedoneproduct":
                                    var productIds = new HashSet<int>();
                                    var failedRequirements = new List<OldDiscountRequirement>();

                                    foreach (var requirement in requirements[name].Where(x => x.RestrictedProductIds.HasValue()))
                                    {
                                        try
                                        {
                                            productIds.AddRange(requirement.RestrictedProductIds.ToIntArray());
                                        }
                                        catch
                                        {
                                            failedRequirements.Add(requirement);
                                        }
                                    }

                                    if (!failedRequirements.Any())
                                    {
                                        if (productIds.Any())
                                        {
                                            ruleSet.Rules.Add(new RuleEntity
                                            {
                                                RuleType = name == "discountrequirement.hasallproducts" || name == "discountrequirement.hasoneproduct" ? "ProductInCart" : "CartPurchasedProduct",
                                                Operator = name == "discountrequirement.hasallproducts" || name == "discountrequirement.purchasedallproducts" ? RuleOperator.Contains : RuleOperator.In,
                                                Value = string.Join(",", productIds)
                                            });
                                        }
                                    }
                                    else
                                    {
                                        var sb = PooledStringBuilder.Rent();
                                        sb.AppendLine($"Cannnot create a rule for '{name}'.");
                                        sb.AppendLine($"Discount name: {discount.Name.NaIfEmpty()}");
                                        sb.AppendLine($"Discount ID: {discount.Id}");
                                        sb.AppendLine();
                                        sb.AppendLine("Unsupported expressions for restricted product IDs:");
                                        failedRequirements.Each(x => sb.AppendLine(x.RestrictedProductIds));

                                        LogWarning($"Cannot migrate requirements for discount '{discount.Name.NaIfEmpty()}'.", sb.ToStringAndReturn());
                                    }
                                    break;
                                case "discountrequirement.store":
                                    foreach (var requirement in requirements[name].Where(x => x.RestrictedToStoreId != 0))
                                    {
                                        ruleSet.Rules.Add(new RuleEntity
                                        {
                                            RuleType = "Store",
                                            Operator = RuleOperator.In,
                                            Value = requirement.RestrictedToStoreId.ToString()
                                        });
                                    }
                                    break;
                                case "discountrequirement.haspaymentmethod":
                                    var paymentMethods = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                                    foreach (var requirement in requirements[name].Where(x => x.RestrictedPaymentMethods.HasValue()))
                                    {
                                        paymentMethods.AddRange(requirement.RestrictedPaymentMethods.SplitSafe(","));
                                    }

                                    if (paymentMethods.Any())
                                    {
                                        ruleSet.Rules.Add(new RuleEntity
                                        {
                                            RuleType = "CartPaymentMethod",
                                            Operator = RuleOperator.In,
                                            Value = string.Join(",", paymentMethods)
                                        });
                                    }
                                    break;
                                case "discountrequirement.hasshippingoption":
                                    var shippingMethodIds = new HashSet<int>();
                                    foreach (var requirement in requirements[name].Where(x => x.RestrictedShippingOptions.HasValue()))
                                    {
                                        shippingMethodIds.AddRange(requirement.RestrictedShippingOptions.ToIntArray());
                                    }

                                    if (shippingMethodIds.Any())
                                    {
                                        ruleSet.Rules.Add(new RuleEntity
                                        {
                                            RuleType = "CartShippingMethod",
                                            Operator = RuleOperator.In,
                                            Value = string.Join(",", shippingMethodIds)
                                        });
                                    }
                                    break;
                                default:
                                    LogWarning($"Cannot add rule set for unknown discount requirement type ({name}).", null);
                                    break;
                            }
                        }
                    }

                    if (ruleSet.Rules.Any())
                    {
                        // Map rule set to discount.
                        discount.RuleSets.Add(ruleSet);
                    }
                }
            }

            void AddRuleSetsForShippingFilters()
            {
                var syncMappings = ctx.Set<SyncMapping>()
                    .AsNoTracking()
                    .Where(x => x.ContextName == "SmartStore.ShippingFilter" && x.EntityName == "ShippingMethod")
                    .ToList()
                    .ToDictionarySafe(x => x.EntityId, x => x);

                if (!syncMappings.Any())
                {
                    return;
                }

                var shippingMethodIds = syncMappings.Select(x => x.Key).ToList();
                var shippingMethods = ctx.Set<ShippingMethod>()
                    .Where(x => shippingMethodIds.Contains(x.Id))
                    .ToList();

                if (!shippingMethods.Any())
                {
                    return;
                }

                foreach (var shippingMethod in shippingMethods)
                {
                    var ruleSetName = shippingNameTemplate.FormatInvariant(shippingMethod.Name.NullEmpty() ?? shippingMethod.Id.ToString()).Truncate(195, "…");

                    // Avoid adding rule sets multiple times.
                    if (!existingRuleSets.TryGetValue(ruleSetName, out var ruleSet))
                    {
                        ruleSet = new RuleSetEntity
                        {
                            Name = ruleSetName,
                            IsActive = true,
                            Scope = RuleScope.Cart,
                            LogicalOperator = LogicalRuleOperator.And,
                            CreatedOnUtc = utcNow,
                            UpdatedOnUtc = utcNow
                        };

                        var model = GetFilterData(syncMappings, shippingMethod.Id, "FilterOutShippingMethodModel");
                        if (model != null)
                        {
                            AddRulesForCommonFilters(ctx, ruleSet, model, "SmartStore.ShippingFilter");
                        }
                    }

                    if (ruleSet.Rules.Any())
                    {
                        // Map rule set to shipping method.
                        shippingMethod.RuleSets.Add(ruleSet);
                    }
                }
            }

            void AddRuleSetsForPaymentFilters()
            {
                var syncMappings = ctx.Set<SyncMapping>()
                    .AsNoTracking()
                    .Where(x => x.ContextName == "SmartStore.PaymentFilter" && x.EntityName == "PaymentMethod")
                    .ToList()
                    .ToDictionarySafe(x => x.EntityId, x => x);

                if (!syncMappings.Any())
                {
                    return;
                }

                var paymentMethodIds = syncMappings.Select(x => x.Key).ToList();
                var paymentMethods = ctx.Set<PaymentMethod>()
                    .Where(x => paymentMethodIds.Contains(x.Id))
                    .ToList();

                if (!paymentMethods.Any())
                {
                    return;
                }

                var stringRessourceSet = ctx.Set<LocaleStringResource>().AsNoTracking();

                foreach (var paymentMethod in paymentMethods)
                {
                    var resourceName = $"Plugins.FriendlyName.{paymentMethod.PaymentMethodSystemName}";
                    var friendlyName = stringRessourceSet
                        .Where(x => x.ResourceName == resourceName && x.LanguageId == defaultLang.Id)
                        .Select(x => x.ResourceValue)
                        .FirstOrDefault()
                        .NullEmpty();

                    var ruleSetName = paymentNameTemplate.FormatInvariant(friendlyName ?? "?", paymentMethod.PaymentMethodSystemName ?? paymentMethod.Id.ToString()).Truncate(195, "…");

                    // Avoid adding rule sets multiple times.
                    if (!existingRuleSets.TryGetValue(ruleSetName, out var ruleSet))
                    {
                        ruleSet = new RuleSetEntity
                        {
                            Name = ruleSetName,
                            IsActive = true,
                            Scope = RuleScope.Cart,
                            LogicalOperator = LogicalRuleOperator.And,
                            CreatedOnUtc = utcNow,
                            UpdatedOnUtc = utcNow
                        };

                        var model = GetFilterData(syncMappings, paymentMethod.Id, "FilterOutPaymentMethodModel");
                        if (model != null)
                        {
                            AddRulesForCommonFilters(ctx, ruleSet, model, "SmartStore.PaymentFilter");

                            // Order number.
                            if (model.HasLessThanOrders.HasValue)
                            {
                                ruleSet.Rules.Add(new RuleEntity
                                {
                                    RuleType = "CartOrderCount",
                                    Operator = RuleOperator.GreaterThanOrEqualTo,
                                    Value = model.HasLessThanOrders.Value.ToString()
                                });
                            }

                            // Spent amount.
                            if (model.HasLessThanTotalAmount.HasValue)
                            {
                                ruleSet.Rules.Add(new RuleEntity
                                {
                                    RuleType = "CartSpentAmount",
                                    Operator = RuleOperator.GreaterThanOrEqualTo,
                                    Value = model.HasLessThanTotalAmount.Value.ToString(CultureInfo.InvariantCulture)
                                });
                            }

                            // Shipping method.
                            if (model.ExcludedShippingMethods?.Any() ?? false)
                            {
                                ruleSet.Rules.Add(new RuleEntity
                                {
                                    RuleType = "CartShippingMethod",
                                    Operator = RuleOperator.NotIn,
                                    Value = string.Join(",", model.ExcludedShippingMethods)
                                });
                            }

                            // Minimum cart amount.
                            if (model.MinimumOrderAmount.HasValue)
                            {
                                ruleSet.Rules.Add(new RuleEntity
                                {
                                    RuleType = model.AmountContext.IsCaseInsensitiveEqual("TotalAmount") ? "CartTotal" : "CartSubtotal",
                                    Operator = RuleOperator.GreaterThanOrEqualTo,
                                    Value = model.MinimumOrderAmount.Value.ToString(CultureInfo.InvariantCulture)
                                });
                            }

                            // Maximum cart amount.
                            if (model.MaximumOrderAmount.HasValue)
                            {
                                ruleSet.Rules.Add(new RuleEntity
                                {
                                    RuleType = model.AmountContext.IsCaseInsensitiveEqual("TotalAmount") ? "CartTotal" : "CartSubtotal",
                                    Operator = RuleOperator.LessThanOrEqualTo,
                                    Value = model.MaximumOrderAmount.Value.ToString(CultureInfo.InvariantCulture)
                                });
                            }
                        }
                    }

                    if (ruleSet.Rules.Any())
                    {
                        // Map rule set to payment method.
                        paymentMethod.RuleSets.Add(ruleSet);
                    }
                }
            }

            T GetExtraData<T>(OldDiscountRequirement req, string name)
            {
                try
                {
                    if (req?.ExtraData?.HasValue() ?? false)
                    {
                        var extraData = JsonConvert.DeserializeObject<Dictionary<string, object>>(req.ExtraData);
                        if (extraData.TryGetValue(name, out var obj))
                        {
                            return obj.Convert<T>(CultureInfo.InvariantCulture);
                        }
                    }
                }
                catch (Exception ex)
                {
                    ex.Dump();
                }

                return default(T);
            }

            PluginFilterConfigModel GetFilterData(Dictionary<int, SyncMapping> syncMappings, int entityId, string rootNodeName)
            {
                try
                {
                    if (syncMappings.TryGetValue(entityId, out var syncMapping) && syncMapping.CustomString.HasValue())
                    {
                        using (var reader = new StringReader(syncMapping.CustomString))
                        {
                            var serializer = new XmlSerializer(typeof(PluginFilterConfigModel), new XmlRootAttribute(rootNodeName));
                            return serializer.Deserialize(reader) as PluginFilterConfigModel;
                        }
                    }
                }
                catch (Exception ex)
                {
                    ex.Dump();
                }

                return null;
            }

            void AddRulesForCommonFilters(SmartObjectContext _ctx, RuleSetEntity ruleSet, PluginFilterConfigModel model, string pluginSystemName)
            {
                // Store id (if plugin is limited to stores).
                var settingName = $"PluginSetting.{pluginSystemName}.LimitedToStores";
                var limitedToStoresValue = _ctx.Set<Setting>()
                    .Where(x => x.Name == settingName)
                    .Select(x => x.Value)
                    .FirstOrDefault();

                if (limitedToStoresValue.HasValue())
                {
                    var storeIds = limitedToStoresValue.ToIntArray();
                    if (storeIds.Any())
                    {
                        ruleSet.Rules.Add(new RuleEntity
                        {
                            RuleType = "Store",
                            Operator = RuleOperator.In,
                            Value = string.Join(",", storeIds)
                        });
                    }
                }

                // Customer role.
                if (model?.ExcludedCustomerRoles?.Any() ?? false)
                {
                    var excludedRoleIds = model.ExcludedCustomerRoles.Where(x => x != 0).ToArray();
                    if (excludedRoleIds.Any())
                    {
                        if (model.FilterIfRoleIsAssigned)
                        {
                            ruleSet.Rules.Add(new RuleEntity
                            {
                                RuleType = "CustomerRole",
                                Operator = RuleOperator.NotContains,
                                Value = string.Join(",", excludedRoleIds)
                            });
                        }
                        else
                        {
                            // There's no operator for this filter. We have to turn him around logically.
                            var allRoleIds = _ctx.Set<CustomerRole>().Select(x => x.Id).ToList();
                            var includedRoleIds = allRoleIds.Except(excludedRoleIds).ToArray();

                            if (includedRoleIds.Any())
                            {
                                ruleSet.Rules.Add(new RuleEntity
                                {
                                    RuleType = "CustomerRole",
                                    Operator = RuleOperator.In,
                                    Value = string.Join(",", includedRoleIds)
                                });
                            }
                        }
                    }
                }

                // Address country.
                if (model.ExcludedCountries?.Any() ?? false)
                {
                    var excludedCountryIds = model.ExcludedCountries.Where(x => x != 0).ToArray();
                    if (excludedCountryIds.Any())
                    {
                        ruleSet.Rules.Add(new RuleEntity
                        {
                            RuleType = model.CountryContext.IsCaseInsensitiveEqual("BillingAddress") ? "CartBillingCountry" : "CartShippingCountry",
                            Operator = RuleOperator.NotIn,
                            Value = string.Join(",", excludedCountryIds)
                        });
                    }
                }
            }

            /*static*/
            static void LogWarning(string message, string fullDescription)
            {
                try
                {
                    var logger = EngineContext.Current.Resolve<ILoggerFactory>().GetLogger(typeof(DataMigrator));
                    if (logger != null)
                    {
                        logger.Warn(fullDescription.HasValue() ? new SmartException(fullDescription) : null, message);
                    }
                }
                catch { }
            }
        }

        public class OldDiscountRequirement
        {
            public int Id { get; set; }
            public int DiscountId { get; set; }
            public string DiscountRequirementRuleSystemName { get; set; }
            public decimal SpentAmount { get; set; }
            public int BillingCountryId { get; set; }
            public int ShippingCountryId { get; set; }
            public int? RestrictedToCustomerRoleId { get; set; }
            public string RestrictedProductIds { get; set; }
            public string RestrictedPaymentMethods { get; set; }
            public string RestrictedShippingOptions { get; set; }
            public int? RestrictedToStoreId { get; set; }
            public string ExtraData { get; set; }
        }

        [Serializable]
        public class PluginFilterConfigModel
        {
            // Values for payment and shipping filter.
            public int[] ExcludedCustomerRoles { get; set; }
            public bool FilterIfRoleIsAssigned { get; set; }
            public int[] ExcludedCountries { get; set; }
            public string CountryContext { get; set; }

            // Values for payment filter.
            public int[] ExcludedShippingMethods { get; set; }
            public decimal? MinimumOrderAmount { get; set; }
            public decimal? MaximumOrderAmount { get; set; }
            public string AmountContext { get; set; }
            public int? HasLessThanOrders { get; set; }
            public decimal? HasLessThanTotalAmount { get; set; }
        }

        #endregion
    }
}
