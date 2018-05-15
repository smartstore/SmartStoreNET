using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using EfState = System.Data.Entity.EntityState;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core;
using SmartStore.Core.Domain.Directory;
using SmartStore.Utilities;
using System.IO;
using System.Xml.Linq;
using SmartStore.Core.Domain.Configuration;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.IO;
using System.Text.RegularExpressions;

namespace SmartStore.Data.Utilities
{
	public static class DataMigrator
	{
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
			return FixProductMainPictureIds(context, false);
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
			var toUpate = new Dictionary<int, int?>();

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
						toUpate.Add(p.Id, fixedPictureId);
					}
				}

				if (!products.HasNextPage)
					break;
			}

			// 2nd pass
			foreach (var chunk in toUpate.Slice(1000))
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

			return toUpate.Count;
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
	}
}
