using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Localization;
using SmartStore.Core.Logging;
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
        private readonly ILogger _logger;

        public GoogleFeedService(
            IRepository<GoogleProductRecord> gpRepository,
            ICommonServices services,
            IPluginFinder pluginFinder,
            ILogger logger)
        {
            _gpRepository = gpRepository;
            _services = services;
            _pluginFinder = pluginFinder;
            _logger = logger;

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
            var insert = (product == null);
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
                case "Export2":
                    product.Export = value.ToBool(true);
                    break;
                case "Multipack2":
                    product.Multipack = value.ToInt();
                    break;
                case "IsBundle":
                    product.IsBundle = (value.IsEmpty() ? (bool?)null : value.ToBool());
                    break;
                case "IsAdult":
                    product.IsAdult = (value.IsEmpty() ? (bool?)null : value.ToBool());
                    break;
                case "EnergyEfficiencyClass":
                    product.EnergyEfficiencyClass = value;
                    break;
                case "CustomLabel0":
                    product.CustomLabel0 = value;
                    break;
                case "CustomLabel1":
                    product.CustomLabel1 = value;
                    break;
                case "CustomLabel2":
                    product.CustomLabel2 = value;
                    break;
                case "CustomLabel3":
                    product.CustomLabel3 = value;
                    break;
                case "CustomLabel4":
                    product.CustomLabel4 = value;
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

            // there's no way to share a context instance across repositories in EF.
            // so we have to fallback to pure SQL here to get the data paged and filtered.

            var hidden = (int)ProductVisibility.Hidden;
            var whereClause = new StringBuilder($"(NOT ([t2].[Deleted] = 1)) AND ([t2].[Visibility] <> {hidden})");

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
                    "SELECT [TotalCount], [t3].[Id], [t3].[Name], [t3].[SKU], [t3].[ProductTypeId], [t3].[value] AS [Taxonomy], [t3].[value2] AS [Gender], [t3].[value3] AS [AgeGroup], [t3].[value4] AS [Color], [t3].[value5] AS [Size], [t3].[value6] AS [Material], [t3].[value7] AS [Pattern], [t3].[value8] AS [Export], [t3].[value9] AS [Multipack], [t3].[value10] AS [IsBundle], [t3].[value11] AS [IsAdult], [t3].[value12] AS [EnergyEfficiencyClass], [t3].[value13] AS [CustomLabel0], [t3].[value14] AS [CustomLabel1], [t3].[value15] AS [CustomLabel2], [t3].[value16] AS [CustomLabel3], [t3].[value17] AS [CustomLabel4]" +
                    " FROM (" +
                    "    SELECT COUNT(id) OVER() [TotalCount], ROW_NUMBER() OVER (ORDER BY [t2].[Name]) AS [ROW_NUMBER], [t2].[Id], [t2].[Name], [t2].[SKU], [t2].[ProductTypeId], [t2].[value], [t2].[value2], [t2].[value3], [t2].[value4], [t2].[value5], [t2].[value6], [t2].[value7], [t2].[value8], [t2].[value9], [t2].[value10], [t2].[value11], [t2].[value12], [t2].[value13], [t2].[value14], [t2].[value15], [t2].[value16], [t2].[value17]" +
                    "    FROM (" +
                    "        SELECT [t0].[Id], [t0].[Name], [t0].[SKU], [t0].[ProductTypeId], [t1].[Taxonomy] AS [value], [t1].[Gender] AS [value2], [t1].[AgeGroup] AS [value3], [t1].[Color] AS [value4], [t1].[Size] AS [value5], [t1].[Material] AS [value6], [t1].[Pattern] AS [value7], COALESCE([t1].[Export],1) AS [value8], COALESCE([t1].[Multipack],0) AS [value9], [t1].[IsBundle] AS [value10], [t1].[IsAdult] AS [value11], [t1].[EnergyEfficiencyClass] AS [value12], [t1].[CustomLabel0] AS [value13], [t1].[CustomLabel1] AS [value14], [t1].[CustomLabel2] AS [value15], [t1].[CustomLabel3] AS [value16], [t1].[CustomLabel4] AS [value17], [t0].[Deleted], [t0].[Visibility], [t1].[IsTouched]" +
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
                    "SELECT [t2].[Id], [t2].[Name], [t2].[SKU], [t2].[ProductTypeId], [t2].[value] AS [Taxonomy], [t2].[value2] AS [Gender], [t2].[value3] AS [AgeGroup], [t2].[value4] AS [Color], [t2].[value5] AS [Size], [t2].[value6] AS [Material], [t2].[value7] AS [Pattern], [t2].[value8] AS [Export], [t2].[value9] AS [Multipack], [t2].[value10] AS [IsBundle], [t2].[value11] AS [IsAdult], [t2].[value12] AS [EnergyEfficiencyClass], [t2].[value13] AS [CustomLabel0], [t2].[value14] AS [CustomLabel1], [t2].[value15] AS [CustomLabel2], [t2].[value16] AS [CustomLabel3], [t2].[value17] AS [CustomLabel4]" +
                    " FROM (" +
                    "     SELECT [t0].[Id], [t0].[Name], [t0].[SKU], [t0].[ProductTypeId], [t1].[Taxonomy] AS [value], [t1].[Gender] AS [value2], [t1].[AgeGroup] AS [value3], [t1].[Color] AS [value4], [t1].[Size] AS [value5], [t1].[Material] AS [value6], [t1].[Pattern] AS [value7], COALESCE([t1].[Export],1) AS [value8], COALESCE([t1].[Multipack],0) AS [value9], [t1].[IsBundle] AS [value10], [t1].[IsAdult] AS [value11], [t1].[EnergyEfficiencyClass] AS [value12], [t1].[CustomLabel0] AS [value13], [t1].[CustomLabel1] AS [value14], [t1].[CustomLabel2] AS [value15], [t1].[CustomLabel3] AS [value16], [t1].[CustomLabel4] AS [value17], [t0].[Deleted], [t0].[Visibility], [t1].[IsTouched] AS [IsTouched]" +
                    "     FROM [Product] AS [t0]" +
                    "     LEFT OUTER JOIN [GoogleProduct] AS [t1] ON [t0].[Id] = [t1].[ProductId]" +
                    " ) AS [t2]" +
                    " WHERE " + whereClause.ToString() +
                    " ORDER BY [t2].[Name]" +
                    " OFFSET {0} ROWS FETCH NEXT {1} ROWS ONLY";

                sqlCount =
                    "SELECT COUNT(*)" +
                    " FROM (" +
                    "     SELECT [t0].[Id], [t0].[Name], [t0].[Deleted], [t0].[Visibility], [t1].[IsTouched] AS [IsTouched]" +
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

                x.Export2Localize = (x.Export == 0 ? no : yes);

                if (x.IsBundle.HasValue)
                    x.IsBundleLocalize = (x.IsBundle.Value ? yes : no);
                else
                    x.IsBundleLocalize = null;

                if (x.IsAdult.HasValue)
                    x.IsAdultLocalize = (x.IsAdult.Value ? yes : no);
                else
                    x.IsAdultLocalize = null;
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

        public List<string> GetTaxonomyList(string searchTerm)
        {
            var result = new List<string>();

            try
            {
                var descriptor = _pluginFinder.GetPluginDescriptorBySystemName(GoogleMerchantCenterFeedPlugin.SystemName);
                var fileDir = Path.Combine(descriptor.Assembly.OriginalFile.Directory.FullName, "Files");
                var fileName = "taxonomy.{0}.txt".FormatInvariant(_services.WorkContext.WorkingLanguage.LanguageCulture ?? "de-DE");
                var path = Path.Combine(fileDir, fileName);
                var filter = searchTerm.HasValue();
                string line;

                if (!File.Exists(path))
                {
                    path = Path.Combine(fileDir, "taxonomy.en-US.txt");
                }

                using (var file = new StreamReader(path, Encoding.UTF8))
                {
                    while ((line = file.ReadLine()) != null)
                    {
                        if (string.IsNullOrWhiteSpace(line))
                        {
                            continue;
                        }
                        if (filter && line.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) < 0)
                        {
                            continue;
                        }

                        result.Add(line);
                    }
                }
            }
            catch (Exception exc)
            {
                _logger.Error(exc);
            }

            return result;
        }
    }
}
