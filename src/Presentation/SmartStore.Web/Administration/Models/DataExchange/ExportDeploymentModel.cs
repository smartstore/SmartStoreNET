using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using FluentValidation.Attributes;
using SmartStore.Admin.Validators.DataExchange;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.DataExchange
{
	[Validator(typeof(ExportDeploymentValidator))]
	public class ExportDeploymentModel : EntityModelBase
	{
		public int ProfileId { get; set; }
		public bool CreateZip { get; set; }
		public string PublicFolderUrl { get; set; }

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

		[SmartResourceDisplayName("Admin.DataExchange.Export.Deployment.SubFolder")]
		public string SubFolder { get; set; }

		[SmartResourceDisplayName("Admin.DataExchange.Export.Deployment.EmailAddresses")]
		public string[] EmailAddresses { get; set; }
		public MultiSelectList AvailableEmailAddresses { get; set; }

		[SmartResourceDisplayName("Admin.DataExchange.Export.Deployment.EmailSubject")]
		public string EmailSubject { get; set; }

		[SmartResourceDisplayName("Admin.DataExchange.Export.Deployment.EmailAccountId")]
		public int EmailAccountId { get; set; }
		public List<SelectListItem> AvailableEmailAccounts { get; set; }

		[SmartResourceDisplayName("Admin.DataExchange.Export.Deployment.PassiveMode")]
		public bool PassiveMode { get; set; }

		[SmartResourceDisplayName("Admin.DataExchange.Export.Deployment.UseSsl")]
		public bool UseSsl { get; set; }

		public LastResultInfo LastResult { get; set; }

		public int FileCount { get; set; }

		public class LastResultInfo
		{
			public DateTime Execution { get; set; }
			public string ExecutionPretty { get; set; }
			public string Error { get; set; }

			public bool Succeeded
			{
				get { return Error.IsEmpty(); }
			}
		}
	}
}