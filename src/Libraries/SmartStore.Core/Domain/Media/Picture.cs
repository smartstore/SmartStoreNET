using System.Collections.Generic;
using SmartStore.Core.Domain.Catalog;
using System.Runtime.Serialization;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartStore.Core.Domain.Media
{
    /// <summary>
    /// Represents a picture
    /// </summary>
	[DataContract]
	public partial class Picture : BaseEntity, ITransient
    {
		public Picture()
		{
			this.UpdatedOnUtc = DateTime.UtcNow;
		}
		
		private ICollection<ProductPicture> _productPictures;
        /// <summary>
        /// Gets or sets the picture binary
        /// </summary>
        public byte[] PictureBinary { get; set; }

        /// <summary>
        /// Gets or sets the picture mime type
        /// </summary>
		[DataMember]
		public string MimeType { get; set; }

        /// <summary>
        /// Gets or sets the SEO friednly filename of the picture
        /// </summary>
		[DataMember]
		public string SeoFilename { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the picture is new
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
        /// Gets or sets the product pictures
        /// </summary>
		[DataMember]
		public virtual ICollection<ProductPicture> ProductPictures
        {
			get { return _productPictures ?? (_productPictures = new HashSet<ProductPicture>()); }
            protected set { _productPictures = value; }
        }
    }
}
