using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace SmartStore.Core.Domain.Media
{
    [DataContract]
    public partial class MediaRegion : MediaFolder
    {
        /// <summary>
        /// Gets or sets the display name resource key.
        /// </summary>
        public string ResKey { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include the folder paths in file URL generation.
        /// </summary>
        [DataMember]
        public bool IncludePath { get; set; }

        /// <summary>
        /// Gets or sets the media region display order.
        /// </summary>
        [DataMember]
        public int? Order { get; set; }

        /// <summary>
        /// Gets or sets the region overlay icon class name.
        /// </summary>
        [DataMember]
        public string Icon { get; set; }

        /// <summary>
        /// Gets or sets the region icon display HTML color
        /// </summary>
        [DataMember]
        public string Color { get; set; }
    }
}
