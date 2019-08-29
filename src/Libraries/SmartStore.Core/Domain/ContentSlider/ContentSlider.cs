using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SmartStore.Core.Domain.ContentSlider
{
    /// <summary>
    /// Represents a ContentSlider
    /// </summary>
    [DataContract]
	public partial class ContentSlider : BaseEntity
	{
		private ICollection<Slide> _slides;
       
        
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
        public virtual ICollection<Slide> Slides
        {
            get { return _slides ?? (_slides = new List<Slide>()); }
            protected set { _slides = value; }
        }
    }
}
