using SmartStore.Core.Domain.Localization;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SmartStore.Core.Domain.ContentSlider
{

    public enum SliderType { HomePageSlider, CategorySlider, ManufacturerSlider };

    /// <summary>
    /// Represents a ContentSlider
    /// </summary>
    [DataContract]
	public partial class ContentSlider : BaseEntity, ILocalizedEntity
    {
        [DataMember]
        public string SliderName { get; set; }
        [DataMember]
        public bool IsActive { get; set; }
		
		[DataMember]
		public bool RandamizeSlides { get; set; }

		[DataMember]
		public bool AutoPlay { get; set; }

        [DataMember]
        public int Delay { get; set; }

		[DataMember]
		public int Height { get; set; }

        [DataMember]
        public int SliderType { get; set; }

        [DataMember]
        public int? ItemId { get; set; }

        [DataMember]
        public virtual ICollection<Slide> Slides { get; set; }
    }
}
