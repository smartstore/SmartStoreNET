namespace SmartStore.Data.Migrations
{
	using System.Collections.Generic;
	using System.Data.Entity.Migrations;
	using System.Linq;
	using Core.Domain.Configuration;
	using Setup;

	public partial class Search : DbMigration, IDataSeeder<SmartObjectContext>
	{
		public override void Up()
        {
        }
        
        public override void Down()
        {
        }

		public bool RollbackOnFailure
		{
			get { return false; }
		}

		public void Seed(SmartObjectContext context)
		{
			var settings = context.Set<Setting>();
			var catalogSettings = settings.Where(x => x.Name.StartsWith("CatalogSettings.")).ToList();
			var searchFields = new List<string>();
			var searchSku = false;
			var searchDescription = false;

			var searchSettingNames = new Dictionary<string, string>
			{
				{ "ProductSearchAutoCompleteEnabled", "InstantSearchEnabled" },
				{ "ProductSearchAutoCompleteNumberOfProducts", "InstantSearchNumberOfProducts" },
				{ "ProductSearchTermMinimumLength", "InstantSearchTermMinLength" },
				{ "SearchPageProductsPerPage", "" },
				{ "ProductSearchPageSizeOptions", "" },
				{ "ProductSearchPageSize", "" },
				{ "ProductSearchAllowCustomersToSelectPageSize", "" },
				{ "SuppressSkuSearch", "" },
				{ "SearchDescriptions", "" },
				{ "ShowProductImagesInSearchAutoComplete", "ShowProductImagesInInstantSearch" }
			};

			// migrate CatalogSettings.DefaultSortOrder 'Position'
			var sortOrderSettings = settings.Where(x => x.Name == "CatalogSettings.DefaultSortOrder" && x.Value == "Position").ToList();
			foreach (var setting in sortOrderSettings)
			{
				setting.Value = "Relevance";
				settings.AddOrUpdate(setting);
			}

			// rename CatalogSettings.ProductsByTagPageSize
			var productsByTagPageSizeSettings = settings.Where(x => x.Name == "CatalogSettings.ProductsByTagPageSize").ToList();
			foreach (var setting in productsByTagPageSizeSettings)
			{
				setting.Name = "CatalogSettings.DefaultProductListPageSize";
				settings.AddOrUpdate(setting);
			}

			context.SaveChanges();

			// delete obsolete catalog settings
			context.MigrateSettings(builder =>
			{
				// migrate catalog settings to search settings
				foreach (var kvp in searchSettingNames.Where(x => x.Value.HasValue()))
				{
					var setting = catalogSettings.FirstOrDefault(x => x.Name.IsCaseInsensitiveEqual("CatalogSettings." + kvp.Key) && x.StoreId == 0);
					if (setting != null && setting.Value.HasValue())
					{
						if (kvp.Key == "SuppressSkuSearch")
						{
							if (!setting.Value.ToBool())
								searchSku = true;
						}
						else if (kvp.Key == "SearchDescriptions")
						{
							if (setting.Value.ToBool())
								searchDescription = true;
						}
						else
						{
							builder.Add("SearchSettings." + kvp.Value, setting.Value);
						}
					}
				}

				// add searchable fields
				if (searchSku)
				{
					searchFields.Add("sku");
				}
				if (searchDescription)
				{
					searchFields.Add("shortdescription");
					searchFields.Add("fulldescription");
				}

				builder.Add("SearchSettings.SearchFields", string.Join(",", searchFields));

				searchSettingNames.Keys.Each(key => builder.Delete("CatalogSettings." + key));
			});
		}
	}
}
