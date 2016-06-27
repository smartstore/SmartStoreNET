namespace SmartStore.Data.Migrations
{
	using System.Collections.Generic;
	using System.Data.Entity.Migrations;
	using System.Linq;
	using Core.Domain;
	using Newtonsoft.Json;
	using Newtonsoft.Json.Linq;
	using Setup;

	public partial class SwapColumnMappingValues : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
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
			context.MigrateLocaleResources(MigrateLocaleResources);

			var importProfiles = context.Set<ImportProfile>().Where(x => x.ColumnMapping != null).ToList();

			foreach (var profile in importProfiles)
			{
				var dic = new Dictionary<string, Dictionary<string, string>>();
				var storeMapping = true;

				try
				{
					var json = JObject.Parse(profile.ColumnMapping);

					foreach (var kvp in json)
					{
						dynamic value = kvp.Value;

						var mappedName = (string)value.MappedName;
						var property = (string)value.Property;
						var defaultValue = (string)value.Default;

						if (mappedName.HasValue())
						{
							// break migration because data is already migrated
							storeMapping = false;
							break;
						}
						else if (property.HasValue())
						{
							if (!kvp.Key.IsCaseInsensitiveEqual(property) || defaultValue.HasValue())
							{
								// swap value
								dic.Add(property, new Dictionary<string, string>
								{
									{ "MappedName", kvp.Key },
									{ "Default", defaultValue }
								});
							}
							else
							{
								// ignore because persisting not required anymore
							}
						}
						else
						{
							// explicitly ignored property
							dic.Add(property, new Dictionary<string, string>
							{
								{ "MappedName", property },
								{ "Default", "[IGNOREPROPERTY]" }
							});
						}
					}
				}
				catch
				{
					storeMapping = true;
					dic.Clear();
				}

				if (storeMapping)
				{
					if (dic.Any())
						profile.ColumnMapping = JsonConvert.SerializeObject(dic);
					else
						profile.ColumnMapping = null;
				}
			}

			context.SaveChanges();
		}

		public void MigrateLocaleResources(LocaleResourcesBuilder builder)
		{
			builder.Delete("Admin.DataExchange.ColumnMapping.Validate.MultipleMappedIgnored");
		}
	}
}
