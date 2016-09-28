using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace SmartStore.Core.Domain.Media
{
    /// <summary>
    /// Represents a download
    /// </summary>
    [DataContract]
	public partial class Download : BaseEntity, ITransient, IHasMedia
	{		
		/// <summary>
        /// Gets or sets a GUID
        /// </summary>
		[DataMember]
		[Index]
		public Guid DownloadGuid { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether DownloadUrl property should be used
        /// </summary>
		[DataMember]
		public bool UseDownloadUrl { get; set; }

        /// <summary>
        /// Gets or sets a download URL
        /// </summary>
		[DataMember]
		public string DownloadUrl { get; set; }

		/// <summary>
		/// Gets or sets the download binary
		/// </summary>
		[Obsolete("Use property MediaStorage instead")]
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

		/// <summary>
		/// Gets or sets a value indicating whether the entity transient/preliminary
		/// </summary>
		[DataMember]
		[Index("IX_UpdatedOn_IsTransient", 1)]
		public bool IsTransient { get; set; }

		/// <summary>
		/// Gets or sets the date and time of instance update
		/// </summary>
		[DataMember]
		[Index("IX_UpdatedOn_IsTransient", 0)]
		public DateTime UpdatedOnUtc { get; set; }

		/// <summary>
		/// Gets or sets the media storage identifier
		/// </summary>
		[DataMember]
		public int? MediaStorageId { get; set; }

		/// <summary>
		/// Gets or sets the media storage
		/// </summary>
		public virtual MediaStorage MediaStorage { get; set; }
	}
}
