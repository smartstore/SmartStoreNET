using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Domain;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Core.Domain.Tasks;
using SmartStore.Core.Events;
using SmartStore.Core.Plugins;
using SmartStore.Services.DataExchange.ExportTask;
using SmartStore.Services.Localization;
using SmartStore.Services.Tasks;
using SmartStore.Utilities;

namespace SmartStore.Services.DataExchange
{
	public partial class ExportService : IExportService
	{
		private const string _defaultFileNamePattern = "%Store.Id%-%ExportProfile.Id%-%Misc.FileNumber%-%ExportProfile.SeoName%";

		private readonly IRepository<ExportProfile> _exportProfileRepository;
		private readonly IRepository<ExportDeployment> _exportDeploymentRepository;
		private readonly IEventPublisher _eventPublisher;
		private readonly IScheduleTaskService _scheduleTaskService;
		private readonly IProviderManager _providerManager;
		private readonly DataExchangeSettings _dataExchangeSettings;
		private readonly ILocalizationService _localizationService;

		public ExportService(
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

		#region Export profiles

		public virtual ExportProfile CreateVolatileProfile(Provider<IExportProvider> provider)
		{
			var name = provider.GetName(_localizationService);
			var seoName = SeoHelper.GetSeName(name, true, false).Replace("/", "").Replace("-", "");

			var profile = new ExportProfile
			{
				Id = 0,
				Name = name,
				FolderName = seoName.ToValidPath().Truncate(_dataExchangeSettings.MaxFileNameLength),
				FileNamePattern = _defaultFileNamePattern,
				ProviderSystemName = provider.Metadata.SystemName,
				Enabled = true,
				SchedulingTaskId = 0,
				PerStore = false,
				CreateZipArchive = false,
				Cleanup = false,
				ScheduleTask = null,	// volatile schedule task impossible cause of database accesses by core
				Deployments = new List<ExportDeployment>()
			};

			// profile.Projection and profile.Filtering should be null here

			return profile;
		}

		public virtual ExportProfile InsertExportProfile(Provider<IExportProvider> provider, int cloneFromProfileId = 0)
		{
			if (provider == null)
				throw new ArgumentNullException("provider");

			var cloneProfile = GetExportProfileById(cloneFromProfileId);
			
			ScheduleTask task = null;
			ExportProfile profile = null;
			var name = provider.GetName(_localizationService);
			var seoName = SeoHelper.GetSeName(name, true, false).Replace("/", "").Replace("-", "");
			
			if (cloneProfile == null)
			{
				task = new ScheduleTask
				{
					CronExpression = "0 */6 * * *",		// every six hours
					Type = (new ExportProfileTask()).GetType().AssemblyQualifiedNameWithoutVersion(),
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

			task.Name = string.Concat(name, " export task");
			
			_scheduleTaskService.InsertTask(task);

			if (cloneProfile == null)
			{
				// what we do here is to preset typical settings for feed creation
				// but on the other hand they may be untypical for generic data export\exchange
				var projection = new ExportProjection
				{
					RemoveCriticalCharacters = true,
					CriticalCharacters = "¼,½,¾",
					PriceType = PriceDisplayType.PreSelectedPrice,
					NoGroupedProducts = (provider.Supports(ExportSupport.ProjectionNoGroupedProducts) ? true : false)
				};

				var filter = new ExportFilter
				{
					IsPublished = true
				};

				profile = new ExportProfile
				{
					FileNamePattern = _defaultFileNamePattern,
					Filtering = XmlHelper.Serialize<ExportFilter>(filter),
					Projection = XmlHelper.Serialize<ExportProjection>(projection)
				};
			}
			else
			{
				profile = cloneProfile.Clone();				
			}

			profile.Name = name;
			profile.FolderName = seoName.ToValidPath().Truncate(_dataExchangeSettings.MaxFileNameLength);
			profile.ProviderSystemName = provider.Metadata.SystemName;
			profile.SchedulingTaskId = task.Id;

			_exportProfileRepository.Insert(profile);

			task.Alias = profile.Id.ToString();
			_scheduleTaskService.UpdateTask(task);

			if (provider.Value.FileExtension.HasValue())
			{
				if (cloneProfile == null)
				{
					if (provider.Supports(ExportSupport.CreateInitialPublicDeployment))
					{
						profile.Deployments.Add(new ExportDeployment
						{
							ProfileId = profile.Id,
							Enabled = true,
							IsPublic = true,
							DeploymentType = ExportDeploymentType.FileSystem,
							Name = profile.Name
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

			_eventPublisher.EntityInserted(profile);

			return profile;
		}

		public virtual void UpdateExportProfile(ExportProfile profile)
		{
			if (profile == null)
				throw new ArgumentNullException("profile");

			_exportProfileRepository.Update(profile);

			_eventPublisher.EntityUpdated(profile);
		}

		public virtual void DeleteExportProfile(ExportProfile profile)
		{
			if (profile == null)
				throw new ArgumentNullException("profile");

			int scheduleTaskId = profile.SchedulingTaskId;
			var folder = profile.GetExportFolder();

			_exportProfileRepository.Delete(profile);

			var scheduleTask = _scheduleTaskService.GetTaskById(scheduleTaskId);
			_scheduleTaskService.DeleteTask(scheduleTask);

			_eventPublisher.EntityDeleted(profile);

			if (System.IO.Directory.Exists(folder))
			{
				FileSystemHelper.ClearDirectory(folder, true);
			}
		}

		public virtual IQueryable<ExportProfile> GetExportProfiles(bool? enabled = null)
		{
			var query =
				from x in _exportProfileRepository.Table.Expand(x => x.ScheduleTask)
				select x;

			if (enabled.HasValue)
			{
				query = query.Where(x => x.Enabled == enabled.Value);
			}

			query = query
				.OrderBy(x => x.ProviderSystemName)
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

		public virtual IList<ExportProfile> GetExportProfilesBySystemName(string systemName)
		{
			if (systemName.IsEmpty())
				return new List<ExportProfile>();

			var profiles = _exportProfileRepository.Table
				.Expand(x => x.ScheduleTask)
				.Expand(x => x.Deployments)
				.Where(x => x.ProviderSystemName == systemName)
				.ToList();

			return profiles;
		}


		public virtual IEnumerable<Provider<IExportProvider>> LoadAllExportProviders(int storeId = 0, bool showHidden = true)
		{
			var allProviders = _providerManager.GetAllProviders<IExportProvider>(storeId)
				.Where(x => x.IsValid() && (showHidden || !x.Metadata.IsHidden))
				.OrderBy(x => x.Metadata.SystemName)
				.ThenBy(x => x.Metadata.FriendlyName);

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

		public virtual void DeleteExportDeployment(ExportDeployment deployment)
		{
			if (deployment == null)
				throw new ArgumentNullException("deployment");

			_exportDeploymentRepository.Delete(deployment);

			_eventPublisher.EntityDeleted(deployment);
		}

		#endregion
	}
}
