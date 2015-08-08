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

		[SmartResourceDisplayName("Admin.Configuration.Export.Deployment.Name")]
		public string Name { get; set; }

		[SmartResourceDisplayName("Common.Enabled")]
		public bool Enabled { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Export.Deployment.DeploymentType")]
		public ExportDeploymentType DeploymentType { get; set; }
		public List<SelectListItem> AvailableDeploymentTypes { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Export.Deployment.DeploymentType")]
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
						return "fa-clone";
					default:
						return "fa-question";
				}
			}
		}


		[SmartResourceDisplayName("Admin.Configuration.Export.Deployment.IsPublic")]
		public bool IsPublic { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Export.Deployment.Username")]
		public string Username { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Export.Deployment.Password")]
		[DataType(DataType.Password)]
		public string Password { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Export.Deployment.Url")]
		public string Url { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Export.Deployment.FileSystemPath")]
		public string FileSystemPath { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Export.Deployment.EmailAddresses")]
		public string EmailAddresses { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Export.Deployment.EmailSubject")]
		public string EmailSubject { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Export.Deployment.EmailAccountId")]
		public int EmailAccountId { get; set; }
		public List<SelectListItem> AvailableEmailAccounts { get; set; }
	}
}