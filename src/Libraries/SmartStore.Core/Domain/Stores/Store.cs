using System.Runtime.Serialization;

namespace SmartStore.Core.Domain.Stores
{
	/// <summary>
	/// Represents a store
	/// </summary>
	[DataContract]
	public partial class Store : BaseEntity
	{
		/// <summary>
		/// Gets or sets the store name
		/// </summary>
		[DataMember]
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets the store URL
		/// </summary>
		[DataMember]
		public string Url { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether SSL is enabled
		/// </summary>
		[DataMember]
		public bool SslEnabled { get; set; }

		/// <summary>
		/// Gets or sets the store secure URL (HTTPS)
		/// </summary>
		[DataMember]
		public string SecureUrl { get; set; }

		/// <summary>
		/// Gets or sets the comma separated list of possible HTTP_HOST values
		/// </summary>
		[DataMember]
		public string Hosts { get; set; }

		/// <summary>
		/// Gets or sets the logo picture id
		/// </summary>
		[DataMember]
		public int LogoPictureId { get; set; }

		/// <summary>
		/// Gets or sets the display order
		/// </summary>
		[DataMember]
		public int DisplayOrder { get; set; }

		/// <summary>
		/// Gets or sets a store specific id for the HTML body
		/// </summary>
		[DataMember]
		public string HtmlBodyId { get; set; }

		/// <summary>
		/// Gets the security mode for the store
		/// </summary>
		public HttpSecurityMode GetSecurityMode(bool? useSsl = null)
		{
			if (useSsl ?? SslEnabled)
			{
				if (SecureUrl.HasValue() && Url.HasValue() && !Url.StartsWith("https"))
				{
					return HttpSecurityMode.SharedSsl;
				}
				else
				{
					return HttpSecurityMode.Ssl;
				}
			}
			return HttpSecurityMode.Unsecured;
		}
	}
}
