using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Localization;
using SmartStore.Core.Plugins;
using SmartStore.GoogleMerchantCenter.Domain;
using SmartStore.GoogleMerchantCenter.Models;
using SmartStore.Services;
using Telerik.Web.Mvc;

namespace SmartStore.GoogleMerchantCenter.Services
{
    public partial class GoogleFeedService : IGoogleFeedService
    {
		private const string _googleNamespace = "http://base.google.com/ns/1.0";

        private readonly IRepository<GoogleProductRecord> _gpRepository;
		private readonly ICommonServices _services;
		private readonly IPluginFinder _pluginFinder;

		public GoogleFeedService(
			IRepository<GoogleProductRecord> gpRepository,
			ICommonServices services,
			IPluginFinder pluginFinder)
        {
            _gpRepository = gpRepository;
			_services = services;
			_pluginFinder = pluginFinder;

			T = NullLocalizer.Instance;
        }

		public Localizer T { get; set; }

        public GoogleProductRecord GetGoogleProductRecord(int productId)
        {
            if (productId == 0)
                return null;

            var query = 
				from gp in _gpRepository.Table
				where gp.ProductId == productId
				select gp;

            var record = query.FirstOrDefault();
            return record;
        }

		public List<GoogleProductRecord> GetGoogleProductRecords(int[] productIds)
		{
			if (productIds == null || productIds.Length == 0)
				return new List<GoogleProductRecord>();

			var lst = _gpRepository.TableUntracked.Where(x => productIds.Contains(x.ProductId)).ToList();
			return lst;
		}

        public void InsertGoogleProductRecord(GoogleProductRecord record)
        {
            if (record == null)
                throw new ArgumentNullException("googleProductRecord");

            _gpRepository.Insert(record);
        }

		public void UpdateGoogleProductRecord(GoogleProductRecord record)
        {
			if (record == null)
				throw new ArgumentNullException("record");

			_gpRepository.Update(record);
        }

		public void DeleteGoogleProductRecord(GoogleProductRecord record)
		{
			if (record == null)
				throw new ArgumentNullException("record");

			_gpRepository.Delete(record);
		}

		public void Upsert(int pk, string name, string value)
		{
			if (pk == 0 || name.IsEmpty())
				return;

			var product = GetGoogleProductRecord(pk);
			bool insert = (product == null);
			var utcNow = DateTime.UtcNow;

			if (product == null)
			{
				product = new GoogleProductRecord
				{
					ProductId = pk,
					CreatedOnUtc = utcNow
				};
			}

			switch (name)
			{
				case "Taxonomy":
					product.Taxonomy = value;
					break;
				case "Gender":
					product.Gender = value;
					break;
				case "AgeGroup":
					product.AgeGroup = value;
					break;
				case "Color":
					product.Color = value;
					break;
				case "Size":
					product.Size = value;
					break;
				case "Material":
					product.Material = value;
					break;
				case "Pattern":
					product.Pattern = value;
					break;
				case "Exporting":
					product.Export = value.ToBool(true);
					break;
			}

			product.UpdatedOnUtc = utcNow;
			product.IsTouched = product.IsTouched();

			if (!insert && !product.IsTouched)
			{
				_gpRepository.Delete(product);
				return;
			}

			if (insert)
			{
				_gpRepository.Insert(product);
			}
			else
			{
				_gpRepository.Update(product);
			}
		}

		public GridModel<GoogleProductModel> GetGridModel(GridCommand command, string searchProductName = null, string touched = null)
		{
			var model = new GridModel<GoogleProductModel>();
			var textInfo = CultureInfo.InvariantCulture.TextInfo;
			string yes = T("Admin.Common.Yes");
			string no = T("Admin.Common.No");

			// there's no way to share a context instance across repositories which makes GoogleProductObjectContext pretty useless here.

			var whereClause = new StringBuilder("(NOT ([t2].[Deleted] = 1)) AND ([t2].[VisibleIndividually] = 1)");

			if (searchProductName.HasValue())
			{
				whereClause.AppendFormat(" AND ([t2].[Name] LIKE '%{0}%')", searchProductName.Replace("'", "''"));
			}

			if (touched.HasValue())
			{
				if (touched.IsCaseInsensitiveEqual("touched"))
					whereClause.Append(" AND ([t2].[IsTouched] = 1)");
				else
					whereClause.Append(" AND ([t2].[IsTouched] = 0 OR [t2].[IsTouched] IS NULL)");
			}

			string sql = null;
			string sqlCount = null;
			var isSqlServer = DataSettings.Current.IsSqlServer;

			if (isSqlServer)
			{
				// fastest possible paged data query
				sql =
					"SELECT [TotalCount], [t3].[Id], [t3].[Name], [t3].[SKU], [t3].[ProductTypeId], [t3].[value] AS [Taxonomy], [t3].[value2] AS [Gender], [t3].[value3] AS [AgeGroup], [t3].[value4] AS [Color], [t3].[value5] AS [Size], [t3].[value6] AS [Material], [t3].[value7] AS [Pattern], [t3].[value8] AS [Export]" +
					" FROM (" +
					"    SELECT COUNT(id) OVER() [TotalCount], ROW_NUMBER() OVER (ORDER BY [t2].[Name]) AS [ROW_NUMBER], [t2].[Id], [t2].[Name], [t2].[SKU], [t2].[ProductTypeId], [t2].[value], [t2].[value2], [t2].[value3], [t2].[value4], [t2].[value5], [t2].[value6], [t2].[value7], [t2].[value8]" +
					"    FROM (" +
					"        SELECT [t0].[Id], [t0].[Name], [t0].[SKU], [t0].[ProductTypeId], [t1].[Taxonomy] AS [value], [t1].[Gender] AS [value2], [t1].[AgeGroup] AS [value3], [t1].[Color] AS [value4], [t1].[Size] AS [value5], [t1].[Material] AS [value6], [t1].[Pattern] AS [value7], COALESCE([t1].[Export],1) AS [value8], [t0].[Deleted], [t0].[VisibleIndividually], [t1].[IsTouched]" +
					"        FROM [Product] AS [t0]" +
					"        LEFT OUTER JOIN [GoogleProduct] AS [t1] ON [t0].[Id] = [t1].[ProductId]" +
					"        ) AS [t2]" +
					"    WHERE " + whereClause.ToString() +
					"    ) AS [t3]" +
					" WHERE [t3].[ROW_NUMBER] BETWEEN {0} + 1 AND {0} + {1}" +
					" ORDER BY [t3].[ROW_NUMBER]";
			}
			else
			{
				// OFFSET... FETCH NEXT requires SQL Server 2012 or SQL CE 4
				sql =
					"SELECT [t2].[Id], [t2].[Name], [t2].[SKU], [t2].[ProductTypeId], [t2].[value] AS [Taxonomy], [t2].[value2] AS [Gender], [t2].[value3] AS [AgeGroup], [t2].[value4] AS [Color], [t2].[value5] AS [Size], [t2].[value6] AS [Material], [t2].[value7] AS [Pattern], [t2].[value8] AS [Export]" +
					" FROM (" +
					"     SELECT [t0].[Id], [t0].[Name], [t0].[SKU], [t0].[ProductTypeId], [t1].[Taxonomy] AS [value], [t1].[Gender] AS [value2], [t1].[AgeGroup] AS [value3], [t1].[Color] AS [value4], [t1].[Size] AS [value5], [t1].[Material] AS [value6], [t1].[Pattern] AS [value7], COALESCE([t1].[Export],1) AS [value8], [t0].[Deleted], [t0].[VisibleIndividually], [t1].[IsTouched] AS [IsTouched]" +
					"     FROM [Product] AS [t0]" +
					"     LEFT OUTER JOIN [GoogleProduct] AS [t1] ON [t0].[Id] = [t1].[ProductId]" +
					" ) AS [t2]" +
					" WHERE " + whereClause.ToString() +
					" ORDER BY [t2].[Name]" +
					" OFFSET {0} ROWS FETCH NEXT {1} ROWS ONLY";

				sqlCount =
					"SELECT COUNT(*)" +
					" FROM (" +
					"     SELECT [t0].[Id], [t0].[Name], [t0].[Deleted], [t0].[VisibleIndividually], [t1].[IsTouched] AS [IsTouched]" +
					"     FROM [Product] AS [t0]" +
					"     LEFT OUTER JOIN [GoogleProduct] AS [t1] ON [t0].[Id] = [t1].[ProductId]" +
					" ) AS [t2]" +
					" WHERE " + whereClause.ToString();
			}


			var data = _gpRepository.Context.SqlQuery<GoogleProductModel>(sql, (command.Page - 1) * command.PageSize, command.PageSize).ToList();

			data.ForEach(x =>
			{
				if (x.ProductType != ProductType.SimpleProduct)
					x.ProductTypeName = T("Admin.Catalog.Products.ProductType.{0}.Label".FormatInvariant(x.ProductType.ToString()));

				if (x.Gender.HasValue())
					x.GenderLocalize = T("Plugins.Feed.Froogle.Gender" + textInfo.ToTitleCase(x.Gender));

				if (x.AgeGroup.HasValue())
					x.AgeGroupLocalize = T("Plugins.Feed.Froogle.AgeGroup" + textInfo.ToTitleCase(x.AgeGroup));

				x.ExportingLocalize = (x.Export == 0 ? no : yes);
			});

			model.Data = data;
			model.Total = (data.Count > 0 ? data.First().TotalCount : 0);

			if (data.Count > 0)
			{
				if (isSqlServer)
					model.Total = data.First().TotalCount;
				else
					model.Total = _gpRepository.Context.SqlQuery<int>(sqlCount).FirstOrDefault();
			}
			else
			{
				model.Total = 0;
			}

			return model;
		}

		public string[] GetTaxonomyList()
		{
			try
			{
				var descriptor = _pluginFinder.GetPluginDescriptorBySystemName(GoogleMerchantCenterFeedPlugin.SystemName);

				var fileDir = Path.Combine(descriptor.OriginalAssemblyFile.Directory.FullName, "Files");
				var fileName = "taxonomy.{0}.txt".FormatWith(_services.WorkContext.WorkingLanguage.LanguageCulture ?? "de-DE");
				var path = Path.Combine(fileDir, fileName);

				if (!File.Exists(path))
					path = Path.Combine(fileDir, "taxonomy.en-US.txt");

				string[] lines = File.ReadAllLines(path, Encoding.UTF8);

				return lines;
			}
			catch (Exception exc)
			{
				exc.Dump();
			}

			return new string[] { };
		}
    }
}
