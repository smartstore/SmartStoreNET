using System;
using System.Runtime.Serialization;

namespace SmartStore.Core.Domain.Media
{
    /// <summary>
    /// Represents a download
    /// </summary>
    [DataContract]
	public partial class Download : BaseEntity
    {
        /// <summary>
        /// Gets a sets a GUID
        /// </summary>
		[DataMember]
		public Guid DownloadGuid { get; set; }

        /// <summary>
        /// Gets a sets a value indicating whether DownloadUrl property should be used
        /// </summary>
		[DataMember]
		public bool UseDownloadUrl { get; set; }

        /// <summary>
        /// Gets a sets a download URL
        /// </summary>
		[DataMember]
		public string DownloadUrl { get; set; }

        /// <summary>
        /// Gets or sets the download binary
        /// </summary>
        public byte[] DownloadBinary { get; set; }

        /// <summary>
        /// The mime-type of the download
        /// </summary>
		[DataMember]
		public string ContentType { get; set; }

        /// <summary>
        /// The filename of the download
        /// </summary>
		[DataMember]
		public string Filename { get; set; }

        /// <summary>
        /// Gets or sets the extension
        /// </summary>
		[DataMember]
		public string Extension { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the download is new
        /// </summary>
		[DataMember]
		public bool IsNew { get; set; }
    }
}
