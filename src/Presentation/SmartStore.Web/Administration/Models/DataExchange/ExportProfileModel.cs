using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Mvc;
using FluentValidation.Attributes;
using SmartStore.Admin.Models.Tasks;
using SmartStore.Admin.Validators.DataExchange;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.DataExchange
{
	[Validator(typeof(ExportProfileValidator))]
	public partial class ExportProfileModel : EntityModelBase
	{	
		public int StoreCount { get; set; }
		public string AllString { get; set; }
		public string UnspecifiedString { get; set; }
		public bool LogFileExists { get; set; }
		public bool HasActiveProvider { get; set; }
		public string[] FileNamePatternDescriptions { get; set; }
		public string PrimaryStoreCurrencyCode { get; set; }

		[SmartResourceDisplayName("Admin.DataExchange.Export.Name")]
		public string Name { get; set; }

		[SmartResourceDisplayName("Admin.DataExchange.Export.SystemName")]
		public string SystemName { get; set; }

		[SmartResourceDisplayName("Admin.DataExchange.Export.IsSystemProfile")]
		public bool IsSystemProfile { get; set; }

		[SmartResourceDisplayName("Admin.DataExchange.Export.ProviderSystemName")]
		public string ProviderSystemName { get; set; }
		public List<ProviderSelectItem> AvailableProviders { get; set; }

		[SmartResourceDisplayName("Admin.DataExchange.Export.FolderName")]
		public string FolderName { get; set; }

		[SmartResourceDisplayName("Admin.DataExchange.Export.FileNamePattern")]
		public string FileNamePattern { get; set; }
		public string FileNamePatternExample { get; set; }

		[SmartResourceDisplayName("Common.Enabled")]
		public bool Enabled { get; set; }

		[SmartResourceDisplayName("Common.Execution")]
		public int ScheduleTaskId { get; set; }
		public string ScheduleTaskName { get; set; }
		public bool IsTaskRunning { get; set; }
		public bool IsTaskEnabled { get; set; }

		[SmartResourceDisplayName("Admin.Common.RecordsSkip")]
		public int Offset { get; set; }

		[SmartResourceDisplayName("Admin.Common.RecordsTake")]
		public int? Limit { get; set; }

		[SmartResourceDisplayName("Admin.DataExchange.Export.BatchSize")]
		public int? BatchSize { get; set; }

		[SmartResourceDisplayName("Admin.DataExchange.Export.PerStore")]
		public bool PerStore { get; set; }

		[SmartResourceDisplayName("Admin.DataExchange.Export.EmailAccountId")]
		public int? EmailAccountId { get; set; }
		public List<SelectListItem> AvailableEmailAccounts { get; set; }

		[SmartResourceDisplayName("Admin.DataExchange.Export.CompletedEmailAddresses")]
		public string[] CompletedEmailAddresses { get; set; }
		public MultiSelectList AvailableCompletedEmailAddresses { get; set; }
		
		[SmartResourceDisplayName("Admin.DataExchange.Export.CreateZipArchive")]
		public bool CreateZipArchive { get; set; }

		[SmartResourceDisplayName("Admin.DataExchange.Export.Cleanup")]
		public bool Cleanup { get; set; }

		[SmartResourceDisplayName("Admin.DataExchange.Export.CloneProfile")]
		public int? CloneProfileId { get; set; }
		public List<ProviderSelectItem> AvailableProfiles { get; set; }

		public ProviderModel Provider { get; set; }

		public ExportFilterModel Filter { get; set; }

		public ExportProjectionModel Projection { get; set; }

		public List<ExportDeploymentModel> Deployments { get; set; }

		public ScheduleTaskModel TaskModel { get; set; }

		public int FileCount { get; set; }

		public class ProviderModel
		{
			public string ConfigPartialViewName { get; set; }
			public Type ConfigDataType { get; set; }
			public object ConfigData { get; set; }

			public ExportFeatures Feature { get; set; }

			[SmartResourceDisplayName("Common.Image")]
			public string ThumbnailUrl { get; set; }

			[SmartResourceDisplayName("Common.Website")]
			public string Url { get; set; }

			[SmartResourceDisplayName("Common.Provider")]
			public string FriendlyName { get; set; }

			[SmartResourceDisplayName("Admin.Configuration.Plugins.Fields.Author")]
			public string Author { get; set; }

			[SmartResourceDisplayName("Admin.Configuration.Plugins.Fields.Version")]
			public string Version { get; set; }

			[SmartResourceDisplayName("Common.Description")]
			[AllowHtml]
			public string Description { get; set; }

			[SmartResourceDisplayName("Admin.DataExchange.Export.EntityType")]
			public ExportEntityType EntityType { get; set; }

			[SmartResourceDisplayName("Admin.DataExchange.Export.EntityType")]
			public string EntityTypeName { get; set; }

			[SmartResourceDisplayName("Admin.DataExchange.Export.FileExtension")]
			public string FileExtension { get; set; }

			public bool IsFileBasedExport
			{
				get { return FileExtension.HasValue(); }
			}

			[SmartResourceDisplayName("Admin.DataExchange.Export.SupportedFileTypes")]
			public string SupportedFileTypes { get; set; }
		}

		public class ProviderSelectItem
		{
			public int Id { get; set; }
			public string SystemName { get; set; }
			public string FriendlyName { get; set; }
			public string ImageUrl { get; set; }
			public string Description { get; set; }
		}
	}


	public partial class ExportFileDetailsModel : EntityModelBase
	{
		public int FileCount
		{
			get
			{
				var result = ExportFiles.Count;

				if (result == 0)
					result = PublicFiles.Count;

				return result;
			}
		}

		public List<FileInfo> ExportFiles { get; set; }
		public List<FileInfo> PublicFiles { get; set; }

		public bool IsForDeployment { get; set; }

		public class FileInfo
		{
			public int StoreId { get; set; }
			public string StoreName { get; set; }
			public string Label { get; set; }

			public int DisplayOrder { get; set; }

			public string FilePath { get; set; }
			public string FileUrl { get; set; }

			public string FileName { get; set; }
			public string FileExtension { get; set; }

			public string FileRootPath
			{
				get
				{
					var rootPath = "";
					var appPath = HttpRuntime.AppDomainAppPath;

					if (FilePath.StartsWith(appPath))
						rootPath = FilePath.Replace(appPath, "~/");

					rootPath = rootPath.Replace('\\', '/');

					return rootPath;
				}
			}
		}
	}
}