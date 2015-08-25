using SmartStore.Core.Domain.DataExchange;

namespace SmartStore.Core.Domain
{
	public class ExportDeployment : BaseEntity
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
		/// Whether to deploy in a folder that can be reached from the internet
		/// </summary>
		public bool IsPublic { get; set; }

		/// <summary>
		/// Whether to create a zip archive with the content of the export
		/// </summary>
		public bool CreateZip { get; set; }

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
		/// Whether to use a multipart form to upload files via HTTP
		/// </summary>
		public bool MultipartForm { get; set; }

		/// <summary>
		/// The file system path
		/// </summary>
		public string FileSystemPath { get; set; }

		/// <summary>
		/// Multiple email addresses can be separated by semicolon
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

		// TODO: more FTP options

		public virtual ExportProfile Profile { get; set; }
	}
}
