using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;
using System.IO;
using System.Linq;
using SmartStore.Core.Data;
using SmartStore.Core.Domain;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Core.Domain.Tasks;
using SmartStore.Core.Events;
using SmartStore.Core.Localization;
using SmartStore.Services.Catalog.Importer;
using SmartStore.Services.Customers.Importer;
using SmartStore.Services.Localization;
using SmartStore.Services.Messages.Importer;
using SmartStore.Services.Tasks;
using SmartStore.Utilities;

namespace SmartStore.Services.DataExchange.Import
{
	public partial class ImportProfileService : IImportProfileService
	{
		private static object _lock = new object();
		private static Dictionary<ImportEntityType, Dictionary<string, string>> _entityProperties = null;

		private readonly IRepository<ImportProfile> _importProfileRepository;
		private readonly IEventPublisher _eventPublisher;
		private readonly IScheduleTaskService _scheduleTaskService;
		private readonly ILocalizationService _localizationService;
		private readonly ILanguageService _languageService;
		private readonly DataExchangeSettings _dataExchangeSettings;

		public ImportProfileService(
			IRepository<ImportProfile> importProfileRepository,
			IEventPublisher eventPublisher,
			IScheduleTaskService scheduleTaskService,
			ILocalizationService localizationService,
			ILanguageService languageService,
			DataExchangeSettings dataExchangeSettings)
		{
			_importProfileRepository = importProfileRepository;
			_eventPublisher = eventPublisher;
			_scheduleTaskService = scheduleTaskService;
			_localizationService = localizationService;
			_languageService = languageService;
			_dataExchangeSettings = dataExchangeSettings;
		}

		private string GetLocalizedPropertyName(ImportEntityType type, string property)
		{
			if (property.IsEmpty())
				return "";

			string key = null;
			string prefixKey = null;

			if (property.StartsWith("BillingAddress."))
				prefixKey = "Admin.Orders.Fields.BillingAddress";
			else if (property.StartsWith("ShippingAddress."))
				prefixKey = "Admin.Orders.Fields.ShippingAddress";

			#region Get resource key

			switch (property)
			{
				case "Id":
					key = "Admin.Common.Entity.Fields.Id";
					break;
				case "LimitedToStores":
					key = "Admin.Common.Store.LimitedTo";
					break;
				case "DisplayOrder":
					key = "Common.DisplayOrder";
					break;
				case "Deleted":
					key = "Admin.Common.Deleted";
					break;
				case "CreatedOnUtc":
				case "BillingAddress.CreatedOnUtc":
				case "ShippingAddress.CreatedOnUtc":
					key = "Common.CreatedOn";
					break;
				case "UpdatedOnUtc":
					key = "Common.UpdatedOn";
					break;
				case "HasDiscountsApplied":
					key = "Admin.Catalog.Products.Fields.HasDiscountsApplied";
					break;
				case "DefaultViewMode":
					key = "Admin.Configuration.Settings.Catalog.DefaultViewMode";
					break;
				case "StoreId":
					key = "Admin.Common.Store";
					break;
				case "ParentGroupedProductId":
					key = "Admin.Catalog.Products.Fields.AssociatedToProductName";
					break;
				case "PasswordFormatId":
					key = "Admin.Configuration.Settings.CustomerUser.DefaultPasswordFormat";
					break;
				case "LastIpAddress":
					key = "Admin.Customers.Customers.Fields.IPAddress";
					break;
				default:
					switch (type)
					{
						case ImportEntityType.Product:
							key = "Admin.Catalog.Products.Fields." + property;
							break;
						case ImportEntityType.Category:
							key = "Admin.Catalog.Categories.Fields." + property;
							break;
						case ImportEntityType.Customer:
							if (property.StartsWith("BillingAddress.") || property.StartsWith("ShippingAddress."))						
								key = "Admin.Address.Fields." + property.Substring(property.IndexOf('.') + 1);
							else
								key = "Admin.Customers.Customers.Fields." + property;
							break;
						case ImportEntityType.NewsLetterSubscription:
							key = "Admin.Promotions.NewsLetterSubscriptions.Fields." + property;
							break;
					}
					break;
			}

			#endregion

			if (key.IsEmpty())
				return "";

			var result = _localizationService.GetResource(key, 0, false, "", true);

			if (result.IsEmpty())
			{
				if (key.EndsWith("Id"))
					result = _localizationService.GetResource(key.Substring(0, key.Length - 2), 0, false, "", true);
				else if (key.EndsWith("Utc"))
					result = _localizationService.GetResource(key.Substring(0, key.Length - 3), 0, false, "", true);
			}

			if (result.IsEmpty())
			{
				Debug.WriteLine("Missing string resource mapping for {0} - {1}".FormatInvariant(type.ToString(), property));
				result = property.SplitPascalCase();
			}

			if (prefixKey.HasValue())
			{
				result = string.Concat(_localizationService.GetResource(prefixKey, 0, false, "", true), " - ", result);
			}

			return result;
		}

		public string GetNewProfileName(ImportEntityType entityType)
		{
			var defaultNames = _localizationService.GetResource("Admin.DataExchange.Import.DefaultProfileNames").SplitSafe(";");

			var result = defaultNames.SafeGet((int)entityType);

			if (result.IsEmpty())
				result = entityType.ToString();

			var profileCount = _importProfileRepository.Table.Count(x => x.EntityTypeId == (int)entityType);

			result = string.Concat(result, " ", profileCount + 1);

			return result;
		}

		public virtual ImportProfile InsertImportProfile(string fileName, string name, ImportEntityType entityType)
		{
			Guard.NotEmpty(fileName, nameof(fileName));

			if (name.IsEmpty())
				name = GetNewProfileName(entityType);

			var task = new ScheduleTask
			{
				CronExpression = "0 */24 * * *",
				Type = typeof(DataImportTask).AssemblyQualifiedNameWithoutVersion(),
				Enabled = false,
				StopOnError = false,
				IsHidden = true
			};

			task.Name = string.Concat(name, " Task");

			_scheduleTaskService.InsertTask(task);

			var profile = new ImportProfile
			{
				Name = name,
				EntityType = entityType,
				Enabled = true,
				SchedulingTaskId = task.Id
			};

			if (Path.GetExtension(fileName).IsCaseInsensitiveEqual(".xlsx"))
				profile.FileType = ImportFileType.XLSX;
			else
				profile.FileType = ImportFileType.CSV;

			string[] keyFieldNames = null;

			switch (entityType)
			{
				case ImportEntityType.Product:
					keyFieldNames = ProductImporter.DefaultKeyFields;
					break;
				case ImportEntityType.Category:
					keyFieldNames = CategoryImporter.DefaultKeyFields;
					break;
				case ImportEntityType.Customer:
					keyFieldNames = CustomerImporter.DefaultKeyFields;
					break;
				case ImportEntityType.NewsLetterSubscription:
					keyFieldNames = NewsLetterSubscriptionImporter.DefaultKeyFields;
					break;
			}

			profile.KeyFieldNames = string.Join(",", keyFieldNames);

			profile.FolderName = SeoHelper.GetSeName(name, true, false)
				.ToValidPath()
				.Truncate(_dataExchangeSettings.MaxFileNameLength);

			var path = DataSettings.Current.TenantPath + "/ImportProfiles";
			profile.FolderName = FileSystemHelper.CreateNonExistingDirectoryName(CommonHelper.MapPath(path), profile.FolderName);

			_importProfileRepository.Insert(profile);

			task.Alias = profile.Id.ToString();
			_scheduleTaskService.UpdateTask(task);

			return profile;
		}

		public virtual void UpdateImportProfile(ImportProfile profile)
		{
			if (profile == null)
				throw new ArgumentNullException("profile");

			_importProfileRepository.Update(profile);
		}

		public virtual void DeleteImportProfile(ImportProfile profile)
		{
			if (profile == null)
				throw new ArgumentNullException("profile");

			var scheduleTaskId = profile.SchedulingTaskId;
			var folder = profile.GetImportFolder();

			_importProfileRepository.Delete(profile);

			var scheduleTask = _scheduleTaskService.GetTaskById(scheduleTaskId);
			_scheduleTaskService.DeleteTask(scheduleTask);

			if (System.IO.Directory.Exists(folder))
			{
				FileSystemHelper.ClearDirectory(folder, true);
			}
		}

		public virtual IQueryable<ImportProfile> GetImportProfiles(bool? enabled = null)
		{
			var query = _importProfileRepository.Table
				.Expand(x => x.ScheduleTask);

			if (enabled.HasValue)
			{
				query = query.Where(x => x.Enabled == enabled.Value);
			}

			query = query
				.OrderBy(x => x.EntityTypeId)
				.ThenBy(x => x.Name);

			return query;
		}

		public virtual ImportProfile GetImportProfileById(int id)
		{
			if (id == 0)
				return null;

			var profile = _importProfileRepository.Table
				.Expand(x => x.ScheduleTask)
				.FirstOrDefault(x => x.Id == id);

			return profile;
		}

		public virtual ImportProfile GetImportProfileByName(string name)
		{
			if (name.IsEmpty())
				return null;

			var profile = _importProfileRepository.Table
				.Expand(x => x.ScheduleTask)
				.FirstOrDefault(x => x.Name == name);

			return profile;
		}

		public virtual Dictionary<string, string> GetImportableEntityProperties(ImportEntityType entityType)
		{
			if (_entityProperties == null)
			{
				lock (_lock)
				{
					if (_entityProperties == null)
					{
						_entityProperties = new Dictionary<ImportEntityType, Dictionary<string, string>>();

						var context = ((IObjectContextAdapter)_importProfileRepository.Context).ObjectContext;
						var container = context.MetadataWorkspace.GetEntityContainer(context.DefaultContainerName, DataSpace.CSpace);

						var allLanguages = _languageService.GetAllLanguages(true);
						var allLanguageNames = allLanguages.ToDictionarySafe(x => x.UniqueSeoCode, x => LocalizationHelper.GetLanguageNativeName(x.LanguageCulture) ?? x.Name);

						var localizableProperties = new Dictionary<ImportEntityType, string[]>
						{
							{ ImportEntityType.Product, new string[] { "Name", "ShortDescription", "FullDescription", "MetaKeywords", "MetaDescription", "MetaTitle", "SeName" } },
							{ ImportEntityType.Category, new string[] { "Name", "FullName", "Description", "BottomDescription", "MetaKeywords", "MetaDescription", "MetaTitle", "SeName" } },
							{ ImportEntityType.Customer, new string[] {  } },
							{ ImportEntityType.NewsLetterSubscription, new string[] {  } }
						};

						var addressSet = container.GetEntitySetByName("Addresses", true);

						var addressProperties = addressSet.ElementType.Members
							.Where(x => !x.Name.IsCaseInsensitiveEqual("Id") && x.BuiltInTypeKind.HasFlag(BuiltInTypeKind.EdmProperty))
							.Select(x => x.Name)
							.ToList();


						foreach (ImportEntityType type in Enum.GetValues(typeof(ImportEntityType)))
						{
							EntitySet entitySet = null;

							try
							{
								if (type == ImportEntityType.Category)
									entitySet = container.GetEntitySetByName("Categories", true);
								else
									entitySet = container.GetEntitySetByName(type.ToString() + "s", true);
							}
							catch (Exception)
							{
								throw new SmartException("There is no entity set for ImportEntityType {0}. Note, the enum value must equal the entity name.".FormatInvariant(type.ToString()));
							}

							var dic = entitySet.ElementType.Members
								.Where(x => !x.Name.IsCaseInsensitiveEqual("Id") && x.BuiltInTypeKind.HasFlag(BuiltInTypeKind.EdmProperty))
								.Select(x => x.Name)
								.ToDictionary(x => x, x => "", StringComparer.OrdinalIgnoreCase);

							// lack of abstractness?
							if ((type == ImportEntityType.Product || type == ImportEntityType.Category) && !dic.ContainsKey("SeName"))
							{
								dic.Add("SeName", "");
							}

							// shipping and billing address
							if (type == ImportEntityType.Customer)
							{
								foreach (var property in addressProperties)
								{
									dic.Add("BillingAddress." + property, "");
									dic.Add("ShippingAddress." + property, "");
								}
							}

							// add localized property names
							foreach (var key in dic.Keys.ToList())
							{
								var localizedValue = GetLocalizedPropertyName(type, key);

								dic[key] = localizedValue.NaIfEmpty();

								if (localizableProperties[type].Contains(key))
								{
									foreach (var language in allLanguages)
									{
										dic.Add(
											"{0}[{1}]".FormatInvariant(key, language.UniqueSeoCode.EmptyNull().ToLower()),
											"{0} {1}".FormatInvariant(localizedValue.NaIfEmpty(), allLanguageNames[language.UniqueSeoCode])
										);
									}
								}
							}

							_entityProperties.Add(type, dic);
						}
					}
				}
			}

			return (_entityProperties.ContainsKey(entityType) ? _entityProperties[entityType] : null);
		}
	}
}
