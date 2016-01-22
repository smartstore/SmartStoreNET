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
using SmartStore.Services.Localization;
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
		private readonly DataExchangeSettings _dataExchangeSettings;

		public ImportProfileService(
			IRepository<ImportProfile> importProfileRepository,
			IEventPublisher eventPublisher,
			IScheduleTaskService scheduleTaskService,
			ILocalizationService localizationService,
			DataExchangeSettings dataExchangeSettings)
		{
			_importProfileRepository = importProfileRepository;
			_eventPublisher = eventPublisher;
			_scheduleTaskService = scheduleTaskService;
			_localizationService = localizationService;
			_dataExchangeSettings = dataExchangeSettings;
		}

		private string GetLocalizedPropertyName(ImportEntityType type, string propertyName)
		{
			if (propertyName.IsEmpty())
				return null;

			var defaultKey = "";
			var keys = new Dictionary<string, string>
			{
				{ "Id", "Admin.Common.Entity.Fields.Id" },
				{ "LimitedToStores", "Admin.Common.Store.LimitedTo" },
				{ "DisplayOrder", "Common.DisplayOrder" },
				{ "Deleted", "Admin.Common.Deleted" },
				{ "CreatedOnUtc", "Common.CreatedOn" },
				{ "UpdatedOnUtc", "Common.UpdatedOn" },
				{ "HasDiscountsApplied", "Admin.Catalog.Products.Fields.HasDiscountsApplied" },
				{ "DefaultViewMode", "Admin.Configuration.Settings.Catalog.DefaultViewMode" },
				{ "StoreId", "Admin.Common.Store" }
			};

			if (type == ImportEntityType.Product)
			{
				defaultKey = "Admin.Catalog.Products.Fields." + propertyName;

				keys.Add("ParentGroupedProductId", "Admin.Catalog.Products.Fields.AssociatedToProductName");
			}
			else if (type == ImportEntityType.Category)
			{
				defaultKey = "Admin.Catalog.Categories.Fields." + propertyName;
			}
			else if (type == ImportEntityType.Customer)
			{
				defaultKey = "Admin.Customers.Customers.Fields." + propertyName;

				keys.Add("PasswordFormatId", "Admin.Configuration.Settings.CustomerUser.DefaultPasswordFormat");
				keys.Add("LastIpAddress", "Admin.Customers.Customers.Fields.IPAddress");
			}
			else if (type == ImportEntityType.NewsLetterSubscription)
			{
				defaultKey = "Admin.Promotions.NewsLetterSubscriptions.Fields." + propertyName;
			}

			var result = _localizationService.GetResource(keys.ContainsKey(propertyName) ? keys[propertyName] : defaultKey, 0, false, "", true);

			if (result.IsEmpty())
			{
				if (defaultKey.EndsWith("Id"))
					result = _localizationService.GetResource(defaultKey.Substring(0, defaultKey.Length - 2), 0, false, "", true);
				else if (defaultKey.EndsWith("Utc"))
					result = _localizationService.GetResource(defaultKey.Substring(0, defaultKey.Length - 3), 0, false, "", true);
			}

			if (result.IsEmpty())
			{
				Debug.WriteLine("Missing string resource mapping for {0}.{1}".FormatInvariant(type.ToString(), propertyName));
				return propertyName.SplitPascalCase();
			}

			return result;
		}

		public virtual ImportProfile InsertImportProfile(string fileName, string name, ImportEntityType entityType)
		{
			Guard.ArgumentNotEmpty(() => fileName);
			Guard.ArgumentNotEmpty(() => name);

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

			profile.FolderName = SeoHelper.GetSeName(name, true, false)
				.ToValidPath()
				.Truncate(_dataExchangeSettings.MaxFileNameLength);

			profile.FolderName = FileSystemHelper.CreateNonExistingDirectoryName(CommonHelper.MapPath("~/App_Data/ImportProfiles"), profile.FolderName);

			_importProfileRepository.Insert(profile);

			task.Alias = profile.Id.ToString();
			_scheduleTaskService.UpdateTask(task);

			_eventPublisher.EntityInserted(profile);

			return profile;
		}

		public virtual void UpdateImportProfile(ImportProfile profile)
		{
			if (profile == null)
				throw new ArgumentNullException("profile");

			var ctx = _importProfileRepository.Context;

			_importProfileRepository.Update(profile);

			_eventPublisher.EntityUpdated(profile);
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

			_eventPublisher.EntityDeleted(profile);

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

						foreach (ImportEntityType type in Enum.GetValues(typeof(ImportEntityType)))
						{
							EntitySet entitySet = null;
							var dic = new Dictionary<string, string>();

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

							foreach (var member in entitySet.ElementType.Members)
							{
								if (member.BuiltInTypeKind.HasFlag(BuiltInTypeKind.EdmProperty))
								{
									var localizedValue = GetLocalizedPropertyName(type, member.Name);

									dic.Add(member.Name, localizedValue.NaIfEmpty());
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
