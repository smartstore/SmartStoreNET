using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Domain;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Core.Domain.Tasks;
using SmartStore.Core.Events;
using SmartStore.Core.Plugins;
using SmartStore.Services.Tasks;
using SmartStore.Utilities;

namespace SmartStore.Services.DataExchange
{
	public partial class ExportService : IExportService
	{
		private readonly IRepository<ExportProfile> _exportProfileRepository;
		private readonly IRepository<ExportDeployment> _exportDeploymentRepository;
		private readonly IEventPublisher _eventPublisher;
		private readonly IScheduleTaskService _scheduleTaskService;
		private readonly IProviderManager _providerManager;
		private readonly DataExchangeSettings _dataExchangeSettings;

		public ExportService(
			IRepository<ExportProfile> exportProfileRepository,
			IRepository<ExportDeployment> exportDeploymentRepository,
			IEventPublisher eventPublisher,
			IScheduleTaskService scheduleTaskService,
			IProviderManager providerManager,
			DataExchangeSettings dataExchangeSettings)
		{
			_exportProfileRepository = exportProfileRepository;
			_exportDeploymentRepository = exportDeploymentRepository;
			_eventPublisher = eventPublisher;
			_scheduleTaskService = scheduleTaskService;
			_providerManager = providerManager;
			_dataExchangeSettings = dataExchangeSettings;
		}

		#region Export profiles

		public virtual ExportProfile InsertExportProfile(Provider<IExportProvider> provider, string name, int cloneFromProfileId = 0)
		{
			if (provider == null)
				throw new ArgumentNullException("provider");

			var cloneProfile = GetExportProfileById(cloneFromProfileId);
			
			var systemName = provider.Metadata.SystemName;

			if (name.IsEmpty())
				name = systemName;

			ScheduleTask task = null;
			ExportProfile profile = null;
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
				profile = new ExportProfile
				{
					FileNamePattern = "%Store.Id%-%ExportProfile.Id%-%Misc.FileNumber%-%ExportProfile.SeoName%",
					Filtering = XmlHelper.Serialize<ExportFilter>(new ExportFilter()),
					Projection = XmlHelper.Serialize<ExportProjection>(new ExportProjection())
				};
			}
			else
			{
				profile = cloneProfile.Clone();				
			}

			profile.Name = name;
			profile.FolderName = seoName.ToValidPath().Truncate(_dataExchangeSettings.MaxFileNameLength);
			profile.ProviderSystemName = systemName;
			profile.SchedulingTaskId = task.Id;

			_exportProfileRepository.Insert(profile);

			task.Alias = profile.Id.ToString();
			_scheduleTaskService.UpdateTask(task);

			if (provider.Value.FileExtension.HasValue())
			{
				if (cloneProfile == null)
				{
					if (systemName.StartsWith("Feeds."))
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

			return query.OrderBy(x => x.Name);
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


		public virtual IEnumerable<Provider<IExportProvider>> LoadAllExportProviders(int storeId = 0)
		{
			var allProviders = _providerManager.GetAllProviders<IExportProvider>(storeId)
				.Where(x => x.IsValid());

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
