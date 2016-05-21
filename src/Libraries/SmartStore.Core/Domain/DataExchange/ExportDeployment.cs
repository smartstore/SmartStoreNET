using System;
using SmartStore.Core.Domain.DataExchange;

namespace SmartStore.Core.Domain
{
	public class ExportDeployment : BaseEntity, ICloneable<ExportDeployment>
	{
		/// <summary>
		/// The profile identifier
		/// </summary>
		public int ProfileId { get; set; }

		/// <summary>
		/// Name of the deployment
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Whether the deployment is enabled
		/// </summary>
		public bool Enabled { get; set; }

		/// <summary>
		/// XML with information about the last deployment result
		/// </summary>
		public string ResultInfo { get; set; }

		/// <summary>
		/// The deployment type identifier
		/// </summary>
		public int DeploymentTypeId { get; set; }

		/// <summary>
		/// The deployment type
		/// </summary>
		public ExportDeploymentType DeploymentType
		{
			get
			{
				return (ExportDeploymentType)DeploymentTypeId;
			}
			set
			{
				DeploymentTypeId = (int)value;
			}
		}

		public string Username { get; set; }

		public string Password { get; set; }

		/// <summary>
		/// Deployment URL
		/// </summary>
		public string Url { get; set; }

		/// <summary>
		/// The type identifier of how to transmit via HTTP
		/// </summary>
		public int HttpTransmissionTypeId { get; set; }

		/// <summary>
		/// The type of how to transmit via HTTP
		/// </summary>
		public ExportHttpTransmissionType HttpTransmissionType
		{
			get
			{
				return (ExportHttpTransmissionType)HttpTransmissionTypeId;
			}
			set
			{
				HttpTransmissionTypeId = (int)value;
			}
		}

		/// <summary>
		/// The file system path
		/// </summary>
		public string FileSystemPath { get; set; }

		/// <summary>
		/// Path of a subfolder
		/// </summary>
		public string SubFolder { get; set; }

		/// <summary>
		/// Multiple email addresses can be separated by commas
		/// </summary>
		public string EmailAddresses { get; set; }

		/// <summary>
		/// Subject of the email
		/// </summary>
		public string EmailSubject { get; set; }

		/// <summary>
		/// Identifier of the email account
		/// </summary>
		public int EmailAccountId { get; set; }

		/// <summary>
		/// Whether to use FTP active or passive mode
		/// </summary>
		public bool PassiveMode { get; set; }

		/// <summary>
		/// Whether to use SSL
		/// </summary>
		public bool UseSsl { get; set; }

		public virtual ExportProfile Profile { get; set; }

		public ExportDeployment Clone()
		{
			var deployment = new ExportDeployment
			{
				Name = this.Name,
				Enabled = this.Enabled,
				DeploymentTypeId = this.DeploymentTypeId,
				Username = this.Username,
				Password = this.Password,
				Url = this.Url,
				HttpTransmissionTypeId = this.HttpTransmissionTypeId,
				FileSystemPath = this.FileSystemPath,
				SubFolder = this.SubFolder,
				EmailAddresses = this.EmailAddresses,
				EmailSubject = this.EmailSubject,
				EmailAccountId = this.EmailAccountId,
				PassiveMode = this.PassiveMode,
				UseSsl = this.UseSsl
			};
			return deployment;
		}

		object ICloneable.Clone()
		{
			return this.Clone();
		}
	}
}
