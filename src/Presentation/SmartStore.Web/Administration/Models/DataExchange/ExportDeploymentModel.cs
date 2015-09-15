using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using FluentValidation.Attributes;
using SmartStore.Admin.Validators.DataExchange;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Mvc;

namespace SmartStore.Admin.Models.DataExchange
{
	[Validator(typeof(ExportDeploymentValidator))]
	public class ExportDeploymentModel : EntityModelBase
	{
		public int ProfileId { get; set; }

		[SmartResourceDisplayName("Common.Image")]
		public string ThumbnailUrl { get; set; }

		[SmartResourceDisplayName("Admin.DataExchange.Export.Deployment.Name")]
		public string Name { get; set; }

		[SmartResourceDisplayName("Common.Enabled")]
		public bool Enabled { get; set; }

		[SmartResourceDisplayName("Admin.DataExchange.Export.Deployment.DeploymentType")]
		public ExportDeploymentType DeploymentType { get; set; }
		public List<SelectListItem> AvailableDeploymentTypes { get; set; }

		[SmartResourceDisplayName("Admin.DataExchange.Export.Deployment.DeploymentType")]
		public string DeploymentTypeName { get; set; }

		public string DeploymentTypeIconClass
		{
			get
			{
				switch (DeploymentType)
				{
					case ExportDeploymentType.FileSystem:
						return "fa-folder-open-o";
					case ExportDeploymentType.Email:
						return "fa-envelope-o";
					case ExportDeploymentType.Http:
						return "fa-globe";
					case ExportDeploymentType.Ftp:
						return "fa-files-o";
					default:
						return "fa-question";
				}
			}
		}

		[SmartResourceDisplayName("Admin.DataExchange.Export.Deployment.CreateZip")]
		public bool CreateZip { get; set; }

		[SmartResourceDisplayName("Admin.DataExchange.Export.Deployment.IsPublic")]
		public bool IsPublic { get; set; }
		public List<PublicFile> PublicFiles { get; set; }

		[SmartResourceDisplayName("Admin.DataExchange.Export.Deployment.Username")]
		public string Username { get; set; }

		[SmartResourceDisplayName("Admin.DataExchange.Export.Deployment.Password")]
		[DataType(DataType.Password)]
		public string Password { get; set; }

		[SmartResourceDisplayName("Admin.DataExchange.Export.Deployment.Url")]
		public string Url { get; set; }

		[SmartResourceDisplayName("Admin.DataExchange.Export.Deployment.HttpTransmissionType")]
		public ExportHttpTransmissionType HttpTransmissionType { get; set; }
		public List<SelectListItem> AvailableHttpTransmissionTypes { get; set; }

		[SmartResourceDisplayName("Admin.DataExchange.Export.Deployment.FileSystemPath")]
		public string FileSystemPath { get; set; }

		[SmartResourceDisplayName("Admin.DataExchange.Export.Deployment.EmailAddresses")]
		public string EmailAddresses { get; set; }
		public string SerializedEmailAddresses { get; set; }

		[SmartResourceDisplayName("Admin.DataExchange.Export.Deployment.EmailSubject")]
		public string EmailSubject { get; set; }

		[SmartResourceDisplayName("Admin.DataExchange.Export.Deployment.EmailAccountId")]
		public int EmailAccountId { get; set; }
		public List<SelectListItem> AvailableEmailAccounts { get; set; }

		[SmartResourceDisplayName("Admin.DataExchange.Export.Deployment.PassiveMode")]
		public bool PassiveMode { get; set; }

		[SmartResourceDisplayName("Admin.DataExchange.Export.Deployment.UseSsl")]
		public bool UseSsl { get; set; }

		public class PublicFile
		{
			public int StoreId { get; set; }
			public string StoreName { get; set; }
			public string FileName { get; set; }
			public string FileUrl { get; set; }
		}
	}
}