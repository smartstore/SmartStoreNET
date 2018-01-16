using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Domain;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Tasks;
using SmartStore.Core.Events;
using SmartStore.Core.Plugins;
using SmartStore.Services.Localization;
using SmartStore.Services.Tasks;
using SmartStore.Utilities;

namespace SmartStore.Services.DataExchange.Export
{
	public partial class ExportProfileService : IExportProfileService
	{
		private const string _defaultFileNamePattern = "%Store.Id%-%Profile.Id%-%File.Index%-%Profile.SeoName%";

		private readonly IRepository<ExportProfile> _exportProfileRepository;
		private readonly IRepository<ExportDeployment> _exportDeploymentRepository;
		private readonly IEventPublisher _eventPublisher;
		private readonly IScheduleTaskService _scheduleTaskService;
		private readonly IProviderManager _providerManager;
		private readonly DataExchangeSettings _dataExchangeSettings;
		private readonly ILocalizationService _localizationService;

		public ExportProfileService(
			IRepository<ExportProfile> exportProfileRepository,
			IRepository<ExportDeployment> exportDeploymentRepository,
			IEventPublisher eventPublisher,
			IScheduleTaskService scheduleTaskService,
			IProviderManager providerManager,
			DataExchangeSettings dataExchangeSettings,
			ILocalizationService localizationService)
		{
			_exportProfileRepository = exportProfileRepository;
			_exportDeploymentRepository = exportDeploymentRepository;
			_eventPublisher = eventPublisher;
			_scheduleTaskService = scheduleTaskService;
			_providerManager = providerManager;
			_dataExchangeSettings = dataExchangeSettings;
			_localizationService = localizationService;
		}

		public virtual ExportProfile InsertExportProfile(
			string providerSystemName,
			string name,
			string fileExtension,
			ExportFeatures features,
			bool isSystemProfile = false,
			string profileSystemName = null,
			int cloneFromProfileId = 0)
		{
			Guard.NotEmpty(providerSystemName, nameof(providerSystemName));

			if (name.IsEmpty())
			{
				name = providerSystemName;
			}

			if (!isSystemProfile)
			{
				var profileCount = _exportProfileRepository.Table.Count(x => x.ProviderSystemName == providerSystemName);

				name = string.Concat(_localizationService.GetResource("Common.My"), " ", name, " ", profileCount + 1);
			}

			var cloneProfile = GetExportProfileById(cloneFromProfileId);

			ScheduleTask task = null;
			ExportProfile profile = null;

			if (cloneProfile == null)
			{
				task = new ScheduleTask
				{
					CronExpression = "0 */6 * * *",     // every six hours
					Type = typeof(DataExportTask).AssemblyQualifiedNameWithoutVersion(),
					Enabled = false,
					StopOnError = false,
					IsHidden = true
				};
			}
			else
			{
				task = cloneProfile.ScheduleTask.Clone();
				task.LastEndUtc = task.LastStartUtc = task.LastSuccessUtc = null;
			}

			task.Name = string.Concat(name, " Task");

			_scheduleTaskService.InsertTask(task);

			if (cloneProfile == null)
			{
				profile = new ExportProfile
				{
					FileNamePattern = _defaultFileNamePattern
				};

				if (isSystemProfile)
				{
					profile.Enabled = true;
					profile.PerStore = false;
					profile.CreateZipArchive = false;
					profile.Cleanup = false;
				}
				else
				{
					// what we do here is to preset typical settings for feed creation
					// but on the other hand they may be untypical for generic data export\exchange
					var projection = new ExportProjection
					{
						RemoveCriticalCharacters = true,
						CriticalCharacters = "¼,½,¾",
						PriceType = PriceDisplayType.PreSelectedPrice,
						NoGroupedProducts = (features.HasFlag(ExportFeatures.CanOmitGroupedProducts) ? true : false),
						OnlyIndividuallyVisibleAssociated = true,
						DescriptionMerging = ExportDescriptionMerging.Description
					};

					var filter = new ExportFilter
					{
						IsPublished = true,
						ShoppingCartTypeId = (int)ShoppingCartType.ShoppingCart
					};

					profile.Projection = XmlHelper.Serialize<ExportProjection>(projection);
					profile.Filtering = XmlHelper.Serialize<ExportFilter>(filter);
				}
			}
			else
			{
				profile = cloneProfile.Clone();
			}

			profile.IsSystemProfile = isSystemProfile;
			profile.Name = name;
			profile.ProviderSystemName = providerSystemName;
			profile.SchedulingTaskId = task.Id;

			var cleanedSystemName = providerSystemName
				.Replace("Exports.", "")
				.Replace("Feeds.", "")
				.Replace("/", "")
				.Replace("-", "");

			var folderName = SeoHelper.GetSeName(cleanedSystemName, true, false)
				.ToValidPath()
				.Truncate(_dataExchangeSettings.MaxFileNameLength);

			var path = DataSettings.Current.TenantPath + "/ExportProfiles";
			profile.FolderName = path + "/" + FileSystemHelper.CreateNonExistingDirectoryName(CommonHelper.MapPath(path), folderName);

			profile.SystemName = profileSystemName.IsEmpty() && isSystemProfile
				? cleanedSystemName
				: profileSystemName;

			_exportProfileRepository.Insert(profile);


			task.Alias = profile.Id.ToString();
			_scheduleTaskService.UpdateTask(task);

			if (fileExtension.HasValue() && !isSystemProfile)
			{
				if (cloneProfile == null)
				{
					if (features.HasFlag(ExportFeatures.CreatesInitialPublicDeployment))
					{
						var subFolder = FileSystemHelper.CreateNonExistingDirectoryName(CommonHelper.MapPath("~/" + DataExporter.PublicFolder), folderName);

						profile.Deployments.Add(new ExportDeployment
						{
							ProfileId = profile.Id,
							Enabled = true,
							DeploymentType = ExportDeploymentType.PublicFolder,
							Name = profile.Name,
							SubFolder = subFolder
						});

						UpdateExportProfile(profile);
					}
				}
				else
				{
					foreach (var deployment in cloneProfile.Deployments)
					{
						profile.Deployments.Add(deployment.Clone());
					}

					UpdateExportProfile(profile);
				}
			}

			return profile;
		}

		public virtual ExportProfile InsertExportProfile(
			Provider<IExportProvider> provider,
			bool isSystemProfile = false,
			string profileSystemName = null,
			int cloneFromProfileId = 0)
		{
			Guard.NotNull(provider, nameof(provider));

			var profile = InsertExportProfile(
				provider.Metadata.SystemName,
				provider.GetName(_localizationService),
				provider.Value.FileExtension,
				provider.Metadata.ExportFeatures,
				isSystemProfile,
				profileSystemName,
				cloneFromProfileId);

			return profile;
		}

		public virtual void UpdateExportProfile(ExportProfile profile)
		{
			if (profile == null)
				throw new ArgumentNullException("profile");

			profile.FolderName = FileSystemHelper.ValidateRootPath(profile.FolderName);

			if (profile.FolderName == "~/")
			{
				throw new SmartException("Invalid export folder name.");
			}

			_exportProfileRepository.Update(profile);
		}

		public virtual void DeleteExportProfile(ExportProfile profile, bool force = false)
		{
			if (profile == null)
				throw new ArgumentNullException("profile");

			if (!force && profile.IsSystemProfile)
				throw new SmartException(_localizationService.GetResource("Admin.DataExchange.Export.CannotDeleteSystemProfile"));

			int scheduleTaskId = profile.SchedulingTaskId;
			var folder = profile.GetExportFolder();

			_exportProfileRepository.Delete(profile);

			var scheduleTask = _scheduleTaskService.GetTaskById(scheduleTaskId);
			_scheduleTaskService.DeleteTask(scheduleTask);

			if (System.IO.Directory.Exists(folder))
			{
				FileSystemHelper.ClearDirectory(folder, true);
			}
		}

		public virtual IQueryable<ExportProfile> GetExportProfiles(bool? enabled = null)
		{
			var query = _exportProfileRepository.Table
				.Expand(x => x.ScheduleTask)
				.Expand(x => x.Deployments);

			if (enabled.HasValue)
			{
				query = query.Where(x => x.Enabled == enabled.Value);
			}

			query = query
				.OrderBy(x => x.IsSystemProfile)
				.ThenBy(x => x.Name);

			return query;
		}

		public virtual ExportProfile GetExportProfileById(int id)
		{
			if (id == 0)
				return null;

			var profile = _exportProfileRepository.Table
				.Expand(x => x.ScheduleTask)
				.Expand(x => x.Deployments)
				.FirstOrDefault(x => x.Id == id);

			return profile;
		}

		public virtual ExportProfile GetSystemExportProfile(string providerSystemName)
		{
			if (providerSystemName.IsEmpty())
				return null;

			var query = GetExportProfiles(true);

			var profile = query
				.Where(x => x.IsSystemProfile && x.ProviderSystemName == providerSystemName)
				.FirstOrDefault();

			return profile;
		}

		public virtual IList<ExportProfile> GetExportProfilesBySystemName(string providerSystemName)
		{
			if (providerSystemName.IsEmpty())
				return new List<ExportProfile>();

			var profiles = _exportProfileRepository.Table
				.Expand(x => x.ScheduleTask)
				.Expand(x => x.Deployments)
				.Where(x => x.ProviderSystemName == providerSystemName)
				.ToList();

			return profiles;
		}


		public virtual IEnumerable<Provider<IExportProvider>> LoadAllExportProviders(int storeId = 0, bool showHidden = true)
		{
			var allProviders = _providerManager.GetAllProviders<IExportProvider>(storeId)
				.Where(x => x.IsValid() && (showHidden || !x.Metadata.IsHidden))
				//.OrderBy(x => x.Metadata.SystemName)
				.OrderBy(x => x.Metadata.FriendlyName);

			return allProviders;
		}

		public virtual Provider<IExportProvider> LoadProvider(string systemName, int storeId = 0)
		{
			var provider = _providerManager.GetProvider<IExportProvider>(systemName, storeId);

			return (provider.IsValid() ? provider : null);
		}

		public virtual ExportDeployment GetExportDeploymentById(int id)
		{
			if (id == 0)
				return null;

			var deployment = _exportDeploymentRepository.Table
				.Expand(x => x.Profile)
				.FirstOrDefault(x => x.Id == id);

			return deployment;
		}

		public virtual void UpdateExportDeployment(ExportDeployment deployment)
		{
			if (deployment == null)
				throw new ArgumentNullException("deployment");

			if (deployment.DeploymentType == ExportDeploymentType.FileSystem && deployment.FileSystemPath == "~/")
			{
				throw new SmartException("Invalid deployment path.");
			}

			_exportDeploymentRepository.Update(deployment);
		}

		public virtual void DeleteExportDeployment(ExportDeployment deployment)
		{
			if (deployment == null)
				throw new ArgumentNullException("deployment");

			_exportDeploymentRepository.Delete(deployment);
		}
	}
}
