using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Media;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SmartStore.Core.Domain.ContentSlider
{
    public enum SlideType { NormalSlide, ProductSlide, CategorySlide, ManufacturerSlide };

    /// <summary>
    /// Represents a ContentSlider
    /// </summary>
    [DataContract]
    public partial class Slide : BaseEntity, ILocalizedEntity
    {
        [DataMember]
        public bool IsActive { get; set; }

        [DataMember]
        public int SliderId { get; set; }

        [DataMember]
        public string SlideTitle { get; set; }
        public string SlideContent { get; set; }

        [DataMember]
        public int PictureId { get; set; }

        [DataMember]
        public int SlideType { get; set; }

		[DataMember]
        public int DisplayOrder { get; set; }

        [DataMember]
        public bool DisplayPrice { get; set; }

        [DataMember]
        public bool DisplayButton { get; set; }

        [DataMember]
        public int? ItemId { get; set; }

        public virtual Picture Picture { get; set; }

        public virtual ContentSlider Slider { get; set; }
    }
}
